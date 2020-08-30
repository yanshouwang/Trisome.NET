using System;
using System.Globalization;
using System.IO;
using Trisome.Core.Logging;

namespace Trisome.WPF.Logging
{
    /// <summary>
    /// Implementation of <see cref="ILogger"/> that logs into a <see cref="TextWriter"/>.
    /// </summary>
    public class TextLogger : ILogger, IDisposable
    {
        /// <summary>
        /// A <see cref="TextWriter"/> that writes to console output.
        /// </summary>
        public TextWriter Writer { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="TextLogger"/> that writes to
        /// the console output.
        /// </summary>
        public TextLogger()
        {
            Writer = Console.Out;
        }

        /// <summary>
        /// Write a new log entry with the specified category and priority.
        /// </summary>
        /// <param name="message">Message body to log.</param>
        /// <param name="category">Category of the entry.</param>
        /// <param name="priority">The priority of the entry.</param>
        public void Log(string message, Category category, Priority priority)
        {
            string logStr = string.Format(
                CultureInfo.InvariantCulture,
                "{0}: {1}. Priority: {2}. Time: {3:U}.",
                category.ToString().ToUpper(CultureInfo.InvariantCulture),
                message,
                priority.ToString(),
                DateTime.Now);

            Writer.WriteLine(logStr);
        }

        /// <summary>
        /// Disposes the associated <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="disposing">When <see langword="true"/>, disposes the associated <see cref="TextWriter"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Writer != null)
                {
                    Writer.Dispose();
                }
            }
        }

        ///<summary>
        ///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        ///</summary>
        /// <remarks>Calls <see cref="Dispose(bool)"/></remarks>.
        ///<filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
