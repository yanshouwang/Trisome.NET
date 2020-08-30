using System;
using System.Collections.Generic;
using System.Text;
using Trisome.WPF.Regions;

namespace Trisome.WPF.Services
{
    public class NavigationService : INavigationService
    {
        readonly IRegionManager _rm;

        public NavigationService(IRegionManager rm)
        {
            _rm = rm;
        }

        public void Navigate(string regionName, string uri)
            => _rm.Navigate(regionName, uri);

        public void Navigate(string regionName, string uri, IDictionary<string, object> args)
            => _rm.Navigate(regionName, uri, args);
    }
}
