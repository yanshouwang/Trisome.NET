using System;
using System.Collections.Generic;
using System.Text;

namespace Trisome.Core.Logging
{
    /// <summary>
    /// Defines a simple logger to be used by the Prism Library.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Write a new log entry with the specified category and priority.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        void Log(string message, Category category, Priority priority);
    }
}
