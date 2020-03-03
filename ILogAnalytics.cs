using System.Collections.Generic;

namespace Siig.DataCollection.LogAnalytics
{
    interface ILogAnalytics
    {
        /// <summary>
        /// Sends a single log entry to Log Analytics
        /// </summary>
        /// <typeparam name="T">Object definition of log entry</typeparam>
        /// <param name="entity">Log entry</param>
        void SendLogEntry<T>(T entity);

        /// <summary>
        /// Sends multiple log entries to Log Analytics
        /// </summary>
        /// <typeparam name="T">Object definition of log entry</typeparam>
        /// <param name="entities">Log Entries</param>
        void SendLogEntries<T>(List<T> entities);
    }
}
