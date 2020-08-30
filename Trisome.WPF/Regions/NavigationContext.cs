using System;
using System.Collections.Generic;
using Trisome.WPF.Common;

namespace Trisome.WPF.Regions
{
    /// <summary>
    /// Encapsulates information about a navigation request.
    /// </summary>
    public class NavigationContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationContext"/> class for a region name and a 
        /// <see cref="Uri"/>.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        /// <param name="uri">The Uri.</param>
        public NavigationContext(IRegionNavigationService navigationService, Uri uri)
            : this(navigationService, uri, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationContext"/> class for a region name and a 
        /// <see cref="Uri"/>.
        /// </summary>
        /// <param name="navigationService">The navigation service.</param>
        /// <param name="navigationParameters">The navigation parameters.</param>
        /// <param name="uri">The Uri.</param>
        public NavigationContext(IRegionNavigationService navigationService, Uri uri, IDictionary<string, object> args)
        {
            NavigationService = navigationService;
            Uri = uri;
            Args = uri != null ? UriParsingHelper.ParseQuery(uri) : null;
            GetNavigationArgs(args);
        }

        /// <summary>
        /// Gets the region navigation service.
        /// </summary>
        /// <value>The navigation service.</value>
        public IRegionNavigationService NavigationService { get; private set; }

        /// <summary>
        /// Gets the navigation URI.
        /// </summary>
        /// <value>The navigation URI.</value>
        public Uri Uri { get; private set; }

        /// <summary>
        /// Gets the <see cref="NavigationParameters"/> extracted from the URI and the object parameters passed in navigation.
        /// </summary>
        /// <value>The URI query.</value>
        public IDictionary<string, object> Args { get; private set; }

        void GetNavigationArgs(IDictionary<string, object> args)
        {
            if (Args == null || NavigationService == null || NavigationService.Region == null)
            {
                Args = new Dictionary<string, object>();
                return;
            }

            if (args != null)
            {
                foreach (var kvPair in args)
                {
                    Args.Add(kvPair.Key, kvPair.Value);
                }
            }
        }
    }
}