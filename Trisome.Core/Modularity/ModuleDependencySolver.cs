using System;
using System.Collections.Generic;
using System.Globalization;
using Trisome.Core.Common;

namespace Trisome.Core.Modularity
{
    /// <summary>
    /// Used by <see cref="IModuleInitializer"/> to get the load sequence
    /// for the modules to load according to their dependencies.
    /// </summary>
    public class ModuleDependencySolver
    {
        readonly ListDictionary<string, string> _dependencyMatrix;
        readonly List<string> _knownModules;

        public ModuleDependencySolver()
        {
            _dependencyMatrix = new ListDictionary<string, string>();
            _knownModules = new List<string>();
        }

        /// <summary>
        /// Adds a module to the solver.
        /// </summary>
        /// <param name="name">The name that uniquely identifies the module.</param>
        public void AddModule(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "The provided String argument {0} must not be null or empty.",
                    "name"));

            AddToDependencyMatrix(name);
            AddToKnownModules(name);
        }

        /// <summary>
        /// Adds a module dependency between the modules specified by dependingModule and
        /// dependentModule.
        /// </summary>
        /// <param name="dependingModule">The name of the module with the dependency.</param>
        /// <param name="dependentModule">The name of the module dependingModule
        /// depends on.</param>
        public void AddDependency(string dependingModule, string dependentModule)
        {
            if (string.IsNullOrEmpty(dependingModule))
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "The provided String argument {0} must not be null or empty.",
                    "dependingModule"));

            if (string.IsNullOrEmpty(dependentModule))
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "The provided String argument {0} must not be null or empty.",
                    "dependentModule"));

            if (!_knownModules.Contains(dependingModule))
                throw new ArgumentException(string.Format(
                    CultureInfo.CurrentCulture,
                    "Cannot add dependency for unknown module {0}",
                    dependingModule));

            AddToDependencyMatrix(dependentModule);
            _dependencyMatrix.Add(dependentModule, dependingModule);
        }

        void AddToDependencyMatrix(string module)
        {
            if (!_dependencyMatrix.ContainsKey(module))
            {
                _dependencyMatrix.Add(module);
            }
        }

        void AddToKnownModules(string module)
        {
            if (!_knownModules.Contains(module))
            {
                _knownModules.Add(module);
            }
        }

        /// <summary>
        /// Calculates an ordered vector according to the defined dependencies.
        /// Non-dependant modules appears at the beginning of the resulting array.
        /// </summary>
        /// <returns>The resulting ordered list of modules.</returns>
        /// <exception cref="CyclicDependencyFoundException">This exception is thrown
        /// when a cycle is found in the defined depedency graph.</exception>
        public string[] Solve()
        {
            List<string> skip = new List<string>();
            while (skip.Count < _dependencyMatrix.Count)
            {
                List<string> leaves = this.FindLeaves(skip);
                if (leaves.Count == 0 && skip.Count < _dependencyMatrix.Count)
                {
                    throw new CyclicDependencyFoundException("At least one cyclic dependency has been found in the module catalog. Cycles in the module dependencies must be avoided.");
                }
                skip.AddRange(leaves);
            }
            skip.Reverse();

            if (skip.Count > _knownModules.Count)
            {
                string moduleNames = this.FindMissingModules(skip);
                throw new ModularityException(moduleNames, string.Format(
                    CultureInfo.CurrentCulture,
                    "A module declared a dependency on another module which is not declared to be loaded. Missing module(s): {0}",
                    moduleNames));
            }

            return skip.ToArray();
        }

        string FindMissingModules(List<string> skip)
        {
            string missingModules = "";

            foreach (string module in skip)
            {
                if (!_knownModules.Contains(module))
                {
                    missingModules += ", ";
                    missingModules += module;
                }
            }

            return missingModules.Substring(2);
        }

        /// <summary>
        /// Gets the number of modules added to the solver.
        /// </summary>
        /// <value>The number of modules.</value>
        public int ModuleCount
        {
            get { return _dependencyMatrix.Count; }
        }

        List<string> FindLeaves(List<string> skip)
        {
            List<string> result = new List<string>();

            foreach (string precedent in _dependencyMatrix.Keys)
            {
                if (skip.Contains(precedent))
                {
                    continue;
                }

                int count = 0;
                foreach (string dependent in _dependencyMatrix[precedent])
                {
                    if (skip.Contains(dependent))
                    {
                        continue;
                    }
                    count++;
                }
                if (count == 0)
                {
                    result.Add(precedent);
                }
            }
            return result;
        }
    }
}