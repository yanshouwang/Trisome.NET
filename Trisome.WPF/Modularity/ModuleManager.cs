using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Trisome.Core.Logging;
using Trisome.Core.Modularity;

namespace Trisome.WPF.Modularity
{
    /// <summary>
    /// Component responsible for coordinating the modules' type loading and module initialization process.
    /// </summary>
    public class ModuleManager : IModuleManager, IDisposable
    {
        readonly IModuleInitializer _moduleInitializer;
        readonly ILogger _logger;
        IEnumerable<IModuleTypeLoader> _typeLoaders;
        readonly HashSet<IModuleTypeLoader> _subscribedToModuleTypeLoaders;

        /// <summary>
        /// The module catalog specified in the constructor.
        /// </summary>
        protected IModuleCatalog ModuleCatalog { get; }

        /// <summary>
        /// Returns the list of registered <see cref="IModuleTypeLoader"/> instances that will be 
        /// used to load the types of modules. 
        /// </summary>
        /// <value>The module type loaders.</value>
        public virtual IEnumerable<IModuleTypeLoader> ModuleTypeLoaders
        {
            get
            {
                if (_typeLoaders == null)
                {
                    _typeLoaders = new List<IModuleTypeLoader> { new FileModuleTypeLoader() };
                }

                return _typeLoaders;
            }
            set
            {
                _typeLoaders = value;
            }
        }

        /// <summary>
        /// Initializes an instance of the <see cref="ModuleManager"/> class.
        /// </summary>
        /// <param name="moduleInitializer">Service used for initialization of modules.</param>
        /// <param name="moduleCatalog">Catalog that enumerates the modules to be loaded and initialized.</param>
        /// <param name="logger">Logger used during the load and initialization of modules.</param>
        public ModuleManager(IModuleInitializer moduleInitializer, IModuleCatalog moduleCatalog, ILogger logger)
        {
            _moduleInitializer = moduleInitializer ?? throw new ArgumentNullException(nameof(moduleInitializer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _typeLoaders = new HashSet<IModuleTypeLoader>();
            _subscribedToModuleTypeLoaders = new HashSet<IModuleTypeLoader>();
            ModuleCatalog = moduleCatalog ?? throw new ArgumentNullException(nameof(moduleCatalog));
        }

        /// <summary>
        /// Raised repeatedly to provide progress as modules are loaded in the background.
        /// </summary>
        public event EventHandler<ModuleDownloadProgressChangedEventArgs> ModuleDownloadProgressChanged;

        void RaiseModuleDownloadProgressChanged(ModuleDownloadProgressChangedEventArgs e)
        {
            ModuleDownloadProgressChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Raised when a module is loaded or fails to load.
        /// </summary>
        public event EventHandler<LoadModuleCompletedEventArgs> LoadModuleCompleted;

        void RaiseLoadModuleCompleted(IModuleInfo moduleInfo, Exception error)
        {
            RaiseLoadModuleCompleted(new LoadModuleCompletedEventArgs(moduleInfo, error));
        }

        void RaiseLoadModuleCompleted(LoadModuleCompletedEventArgs e)
        {
            LoadModuleCompleted?.Invoke(this, e);
        }

        /// <summary>
        /// Initializes the modules marked as <see cref="InitializationMode.WhenAvailable"/> on the <see cref="ModuleCatalog"/>.
        /// </summary>
        public void Run()
        {
            ModuleCatalog.Initialize();
            LoadModulesWhenAvailable();
        }

        /// <summary>
        /// Loads and initializes the module on the <see cref="ModuleCatalog"/> with the name <paramref name="moduleName"/>.
        /// </summary>
        /// <param name="moduleName">Name of the module requested for initialization.</param>
        public void LoadModule(string moduleName)
        {
            var module = ModuleCatalog.Modules.Where(m => m.ModuleName == moduleName);
            if (module == null || module.Count() != 1)
            {
                throw new ModuleNotFoundException(
                    moduleName,
                    string.Format(
                        CultureInfo.CurrentCulture,
                        "Module {0} was not found in the catalog.",
                        moduleName));
            }

            var modulesToLoad = this.ModuleCatalog.CompleteListWithDependencies(module);

            LoadModuleTypes(modulesToLoad);
        }

        /// <summary>
        /// Checks if the module needs to be retrieved before it's initialized.
        /// </summary>
        /// <param name="moduleInfo">Module that is being checked if needs retrieval.</param>
        /// <returns></returns>
        protected virtual bool ModuleNeedsRetrieval(IModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
                throw new ArgumentNullException(nameof(moduleInfo));

            if (moduleInfo.State == ModuleState.NotStarted)
            {
                // If we can instantiate the type, that means the module's assembly is already loaded into
                // the AppDomain and we don't need to retrieve it.
                bool isAvailable = Type.GetType(moduleInfo.ModuleType) != null;
                if (isAvailable)
                {
                    moduleInfo.State = ModuleState.ReadyForInitialization;
                }

                return !isAvailable;
            }

            return false;
        }

        void LoadModulesWhenAvailable()
        {
            var whenAvailableModules = this.ModuleCatalog.Modules.Where(m => m.InitializationMode == InitializationMode.WhenAvailable);
            var modulesToLoadTypes = this.ModuleCatalog.CompleteListWithDependencies(whenAvailableModules);
            if (modulesToLoadTypes != null)
            {
                this.LoadModuleTypes(modulesToLoadTypes);
            }
        }

        void LoadModuleTypes(IEnumerable<IModuleInfo> moduleInfos)
        {
            if (moduleInfos == null)
            {
                return;
            }

            foreach (var moduleInfo in moduleInfos)
            {
                if (moduleInfo.State == ModuleState.NotStarted)
                {
                    if (ModuleNeedsRetrieval(moduleInfo))
                    {
                        BeginRetrievingModule(moduleInfo);
                    }
                    else
                    {
                        moduleInfo.State = ModuleState.ReadyForInitialization;
                    }
                }
            }

            LoadModulesThatAreReadyForLoad();
        }

        /// <summary>
        /// Loads the modules that are not intialized and have their dependencies loaded.
        /// </summary>
        protected virtual void LoadModulesThatAreReadyForLoad()
        {
            bool keepLoading = true;
            while (keepLoading)
            {
                keepLoading = false;
                var availableModules = this.ModuleCatalog.Modules.Where(m => m.State == ModuleState.ReadyForInitialization);

                foreach (var moduleInfo in availableModules)
                {
                    if ((moduleInfo.State != ModuleState.Initialized) && (AreDependenciesLoaded(moduleInfo)))
                    {
                        moduleInfo.State = ModuleState.Initializing;
                        InitializeModule(moduleInfo);
                        keepLoading = true;
                        break;
                    }
                }
            }
        }

        void BeginRetrievingModule(IModuleInfo moduleInfo)
        {
            var moduleInfoToLoadType = moduleInfo;
            var moduleTypeLoader = GetTypeLoaderForModule(moduleInfoToLoadType);
            moduleInfoToLoadType.State = ModuleState.LoadingTypes;

            // Delegate += works differently betweem SL and WPF.
            // We only want to subscribe to each instance once.
            if (!_subscribedToModuleTypeLoaders.Contains(moduleTypeLoader))
            {
                moduleTypeLoader.ModuleDownloadProgressChanged += this.IModuleTypeLoader_ModuleDownloadProgressChanged;
                moduleTypeLoader.LoadModuleCompleted += this.IModuleTypeLoader_LoadModuleCompleted;
                _subscribedToModuleTypeLoaders.Add(moduleTypeLoader);
            }

            moduleTypeLoader.LoadModuleType(moduleInfo);
        }

        void IModuleTypeLoader_ModuleDownloadProgressChanged(object sender, ModuleDownloadProgressChangedEventArgs e)
        {
            RaiseModuleDownloadProgressChanged(e);
        }

        void IModuleTypeLoader_LoadModuleCompleted(object sender, LoadModuleCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if ((e.ModuleInfo.State != ModuleState.Initializing) && (e.ModuleInfo.State != ModuleState.Initialized))
                {
                    e.ModuleInfo.State = ModuleState.ReadyForInitialization;
                }

                // This callback may call back on the UI thread, but we are not guaranteeing it.
                // If you were to add a custom retriever that retrieved in the background, you
                // would need to consider dispatching to the UI thread.
                LoadModulesThatAreReadyForLoad();
            }
            else
            {
                RaiseLoadModuleCompleted(e);

                // If the error is not handled then I log it and raise an exception.
                if (!e.IsErrorHandled)
                {
                    HandleModuleTypeLoadingError(e.ModuleInfo, e.Error);
                }
            }
        }

        /// <summary>
        /// Handles any exception occurred in the module typeloading process,
        /// logs the error using the <see cref="ILoggerFacade"/> and throws a <see cref="ModuleTypeLoadingException"/>.
        /// This method can be overridden to provide a different behavior.
        /// </summary>
        /// <param name="moduleInfo">The module metadata where the error happenened.</param>
        /// <param name="exception">The exception thrown that is the cause of the current error.</param>
        /// <exception cref="ModuleTypeLoadingException"></exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
        protected virtual void HandleModuleTypeLoadingError(IModuleInfo moduleInfo, Exception exception)
        {
            if (moduleInfo == null)
                throw new ArgumentNullException(nameof(moduleInfo));


            if (!(exception is ModuleTypeLoadingException moduleTypeLoadingException))
            {
                moduleTypeLoadingException = new ModuleTypeLoadingException(moduleInfo.ModuleName, exception.Message, exception);
            }

            _logger.Log(moduleTypeLoadingException.Message, Category.Error, Priority.High);

            throw moduleTypeLoadingException;
        }

        bool AreDependenciesLoaded(IModuleInfo moduleInfo)
        {
            var requiredModules = this.ModuleCatalog.GetDependentModules(moduleInfo);
            if (requiredModules == null)
            {
                return true;
            }

            int notReadyRequiredModuleCount =
                requiredModules.Count(requiredModule => requiredModule.State != ModuleState.Initialized);

            return notReadyRequiredModuleCount == 0;
        }

        IModuleTypeLoader GetTypeLoaderForModule(IModuleInfo moduleInfo)
        {
            foreach (IModuleTypeLoader typeLoader in ModuleTypeLoaders)
            {
                if (typeLoader.CanLoadModuleType(moduleInfo))
                {
                    return typeLoader;
                }
            }

            throw new ModuleTypeLoaderNotFoundException(
                moduleInfo.ModuleName,
                string.Format(
                    CultureInfo.CurrentCulture,
                    "There is currently no moduleTypeLoader in the ModuleManager that can retrieve the specified module.",
                    moduleInfo.ModuleName),
                null);
        }

        void InitializeModule(IModuleInfo moduleInfo)
        {
            if (moduleInfo.State == ModuleState.Initializing)
            {
                _moduleInitializer.Initialize(moduleInfo);
                moduleInfo.State = ModuleState.Initialized;
                RaiseLoadModuleCompleted(moduleInfo, null);
            }
        }

        #region IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <remarks>Calls <see cref="Dispose(bool)"/></remarks>.
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the associated <see cref="IModuleTypeLoader"/>s.
        /// </summary>
        /// <param name="disposing">When <see langword="true"/>, it is being called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            foreach (IModuleTypeLoader typeLoader in this.ModuleTypeLoaders)
            {
                if (typeLoader is IDisposable disposableTypeLoader)
                {
                    disposableTypeLoader.Dispose();
                }
            }
        }

        #endregion
    }
}
