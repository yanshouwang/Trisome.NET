using System.Collections.ObjectModel;
using Trisome.Core.Modularity;

namespace Trisome.WPF.Modularity
{
    /// <summary>
    /// 
    /// </summary>
    public interface IModuleGroupsCatalog
    {
        Collection<IModuleCatalogItem> Items { get; }
    }
}