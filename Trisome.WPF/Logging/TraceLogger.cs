using System.Diagnostics;
using Trisome.Core.Logging;

namespace Trisome.WPF.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILogger"/> that logs to .NET <see cref="Trace"/> class.
    /// </summary>
    public class TraceLogger : ILogger
    {
        /// <summary>
        /// Write a new log entry with the specified category and priority.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        public void Log(string message, Category category, Priority priority)
        {
            if (category == Category.Error)
            {
                Trace.TraceError(message);
            }
            else
            {
                Trace.TraceInformation(message);
            }
        }
    }
}
