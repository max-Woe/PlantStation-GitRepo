using System; // Required for the 'Exception' type

namespace LoggingService
{
    /// <summary>
    /// Defines a contract for a service responsible for logging different types of application events, 
    /// including success, errors, database exceptions, and general information, with support for operation timing.
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// Starts a timer, typically used to measure the duration of a subsequent operation.
        /// </summary>
        void StartTimer();

        /// <summary>
        /// Logs a successful operation completion.
        /// </summary>
        /// <param name="operation">A descriptive name of the operation that was successfully completed (e.g., "Data Fetch").</param>
        /// <param name="method">The name of the method where the success occurred (e.g., "GetStationsAsync").</param>
        /// <param name="entity">The entity or data object related to the operation, or <see langword="null"/> if not applicable.</param>
        void LogSuccess(string operation, string method, object? entity);

        /// <summary>
        /// Logs an error that occurred during an operation.
        /// </summary>
        /// <param name="ex">The exception object that was caught.</param>
        /// <param name="operation">A descriptive name of the operation that failed (e.g., "Data Fetch").</param>
        /// <param name="method">The name of the method where the error occurred (e.g., "GetStationsAsync").</param>
        /// <param name="entity">The entity or data object related to the failed operation, or <see langword="null"/> if not applicable.</param>
        void LogError(Exception ex, string operation, string method, object? entity);

        /// <summary>
        /// Stops the previously started timer and typically logs the duration of the timed operation.
        /// </summary>
        void StopTimer();

        /// <summary>
        /// Logs a specific database-related exception.
        /// </summary>
        /// <param name="ex">The database exception object (e.g., <c>DbUpdateException</c>, <c>SqlException</c>).</param>
        void LogDBError(Exception ex);

        /// <summary>
        /// Logs a simple, general informational message.
        /// </summary>
        /// <param name="logText">The informational text to be logged.</param>
        void LogInformationText(string logText);
    }
}