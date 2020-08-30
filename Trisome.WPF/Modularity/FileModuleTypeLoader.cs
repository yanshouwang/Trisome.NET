using System;
using System.Collections.Generic;
using System.IO;
using Trisome.Core.Modularity;

namespace Trisome.WPF.Modularity
{
    /// <summary>
    /// Loads modules from an arbitrary location on the filesystem. This typeloader is only called if
    /// <see cref="ModuleInfo"/> classes have a Ref parameter that starts with "file://".
    /// This class is only used on the Desktop version of the Prism Library.
    /// </summary>
    public class FileModuleTypeLoader : IModuleTypeLoader, IDisposable
    {
        const string REF_FILE_PREFIX = "file://";

        readonly IAssemblyResolver _assemblyResolver;
        readonly HashSet<Uri> _downloadedUris;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileModuleTypeLoader"/> class.
        /// </summary>
        public FileModuleTypeLoader()
            : this(new AssemblyResolver())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileModuleTypeLoader"/> class.
        /// </summary>
        /// <param name="assemblyResolver">The assembly resolver.</param>
        public FileModuleTypeLoader(IAssemblyResolver assemblyResolver)
        {
            _assemblyResolver = assemblyResolver;
            _downloadedUris = new HashSet<Uri>();
        }

        /// <summary>
        /// Raised repeatedly to provide progress as modules are loaded in the background.
        /// </summary>
        public event EventHandler<ModuleDownloadProgressChangedEventArgs> ModuleDownloadProgressChanged;

        void RaiseModuleDownloadProgressChanged(IModuleInfo moduleInfo, long bytesReceived, long totalBytesToReceive)
        {
            RaiseModuleDownloadProgressChanged(new ModuleDownloadProgressChangedEventArgs(moduleInfo, bytesReceived, totalBytesToReceive));
        }

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
        /// Evaluates the <see cref="IModuleInfo.Ref"/> property to see if the current typeloader will be able to retrieve the <paramref name="moduleInfo"/>.
        /// Returns true if the <see cref="IModuleInfo.Ref"/> property starts with "file://", because this indicates that the file
        /// is a local file.
        /// </summary>
        /// <param name="moduleInfo">Module that should have it's type loaded.</param>
        /// <returns>
        /// 	<see langword="true"/> if the current typeloader is able to retrieve the module, otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">An <see cref="ArgumentNullException"/> is thrown if <paramref name="moduleInfo"/> is null.</exception>
        public bool CanLoadModuleType(IModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException(nameof(moduleInfo));
            }

            return moduleInfo.Ref != null && moduleInfo.Ref.StartsWith(REF_FILE_PREFIX, StringComparison.Ordinal);
        }

        /// <summary>
        /// Retrieves the <paramref name="moduleInfo"/>.
        /// </summary>
        /// <param name="moduleInfo">Module that should have it's type loaded.</param>
        public void LoadModuleType(IModuleInfo moduleInfo)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException(nameof(moduleInfo));
            }
            try
            {
                var uri = new Uri(moduleInfo.Ref, UriKind.RelativeOrAbsolute);
                // If this module has already been downloaded, I fire the completed event.
                if (IsSuccessfullyDownloaded(uri))
                {
                    RaiseLoadModuleCompleted(moduleInfo, null);
                }
                else
                {
                    var path = uri.LocalPath;
                    var fileSize = -1L;
                    if (File.Exists(path))
                    {
                        var fileInfo = new FileInfo(path);
                        fileSize = fileInfo.Length;
                    }
                    // Although this isn't asynchronous, nor expected to take very long, I raise progress changed for consistency.
                    RaiseModuleDownloadProgressChanged(moduleInfo, 0, fileSize);
                    _assemblyResolver.LoadAssemblyFrom(moduleInfo.Ref);
                    // Although this isn't asynchronous, nor expected to take very long, I raise progress changed for consistency.
                    RaiseModuleDownloadProgressChanged(moduleInfo, fileSize, fileSize);
                    // I remember the downloaded URI.
                    RecordDownloadSuccess(uri);
                    RaiseLoadModuleCompleted(moduleInfo, null);
                }
            }
            catch (Exception ex)
            {
                this.RaiseLoadModuleCompleted(moduleInfo, ex);
            }
        }

        bool IsSuccessfullyDownloaded(Uri uri)
        {
            lock (_downloadedUris)
            {
                return _downloadedUris.Contains(uri);
            }
        }

        void RecordDownloadSuccess(Uri uri)
        {
            lock (_downloadedUris)
            {
                _downloadedUris.Add(uri);
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
        /// Disposes the associated <see cref="AssemblyResolver"/>.
        /// </summary>
        /// <param name="disposing">When <see langword="true"/>, it is being called from the Dispose method.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_assemblyResolver is IDisposable disposableResolver)
            {
                disposableResolver.Dispose();
            }
        }

        #endregion
    }
}