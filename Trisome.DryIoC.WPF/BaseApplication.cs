using DryIoc;
using System;
using Trisome.Core.IoC;
using Trisome.WPF.Extension;
using TWBaseApplication = Trisome.WPF.BaseApplication;

namespace Trisome.DryIoC.WPF
{
    public abstract class BaseApplication : TWBaseApplication
    {
        /// <summary>
        /// Create <see cref="Rules" /> to alter behavior of <see cref="IContainer" />
        /// </summary>
        /// <returns>An instance of <see cref="Rules" /></returns>
        protected virtual Rules CreateContainerRules()
            => ContainerExtension.DefaultRules;

        /// <summary>
        /// Create a new <see cref="ContainerExtension"/> used by Prism.
        /// </summary>
        /// <returns>A new <see cref="ContainerExtension"/>.</returns>
        protected override IContainerExtension CreateContainerExtension()
        {
            return new ContainerExtension(new Container(CreateContainerRules()));
        }

        /// <summary>
        /// Registers the <see cref="Type"/>s of the Exceptions that are not considered 
        /// root exceptions by the <see cref="ExceptionExtensions"/>.
        /// </summary>
        protected override void RegisterFrameworkExceptionTypes()
        {
            ExceptionExtensions.RegisterFrameworkExceptionType(typeof(ContainerException));
        }
    }
}
