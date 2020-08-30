using System.Collections.Generic;

namespace Trisome.WPF.Services
{
    public interface INavigationService
    {
        void Navigate(string regionName, string uri);
        void Navigate(string regionName, string uri, IDictionary<string, object> args);
    }
}
