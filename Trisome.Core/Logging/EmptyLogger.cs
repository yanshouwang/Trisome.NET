using System;
using System.Collections.Generic;
using System.Text;

namespace Trisome.Core.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILogger"/> that does nothing. This
    /// implementation is useful when the application does not need logging
    /// but there are infrastructure pieces that assume there is a logger.
    /// </summary>
    public class EmptyLogger : ILogger
    {
        /// <summary>
        /// This method does nothing.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        public void Log(string message, Category category, Priority priority)
        {

        }
    }
}
