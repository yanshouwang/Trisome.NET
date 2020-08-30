using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace Trisome.Core.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILogger"/> that logs into a message into the Debug.Listeners collection.
    /// </summary>
    public class DebugLogger : ILogger
    {
        /// <summary>
        /// Write a new log entry with the specified category and priority.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        public void Log(string message, Category category, Priority priority)
        {
            var logStr = string.Format(
                CultureInfo.InvariantCulture,
                "{0}: {1}. Priority: {2}. Time:{3:U}.",
                category.ToString().ToUpper(),
                message,
                priority,
                DateTime.Now);

            Debug.WriteLine(logStr);
        }
    }
}
