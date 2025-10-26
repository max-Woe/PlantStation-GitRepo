using System.Diagnostics;
using Serilog;

namespace LoggingService
{
    /// <summary>
    /// An implementation of the <see cref="ILoggingService"/> interface that uses the Serilog library 
    /// for logging output. It also incorporates a <see cref="Stopwatch"/> to measure and log operation duration.
    /// </summary>
    public class SeriLoggingService : ILoggingService
    {
        Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// Logs a database-related error using Serilog's <see cref="Serilog.Log.Error(Exception, string, object[])"/> method.
        /// The exception message is used as the logging message.
        /// </summary>
        /// <param name="ex">The database exception to log.</param>
        public void LogDBError(Exception ex)
        {
            Serilog.Log.Error(ex, ex.Message);
        }

        /// <summary>
        /// Initializes and starts the internal <see cref="Stopwatch"/> to begin timing an operation.
        /// </summary>
        public void StartTimer()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Logs a successful operation completion using Serilog's Information level.
        /// The log includes the operation name, method name, entity details (if provided), and the time elapsed since the timer started.
        /// </summary>
        /// <param name="operation">A descriptive name of the operation that was successfully completed.</param>
        /// <param name="method">The name of the method where the success occurred.</param>
        /// <param name="entity">The entity or data object related to the operation, or <see langword="null"/>.</param>
        public void LogSuccess(string operation, string method, object? entity)
        {
            if(entity == null)
            {
                Serilog.Log.Information($"Successful: '{operation}' was executed in Methode: '{method}' after '{_stopwatch.ElapsedMilliseconds}' ms.");
                return;
            }
            Serilog.Log.Information($"Successful: '{operation}' was executed on Entity: '{entity.ToString()}' in Methode: '{method}' after '{_stopwatch.ElapsedMilliseconds}' ms.");
        }

        /// <summary>
        /// Logs an error that occurred during an operation using Serilog's Error level.
        /// The log includes the exception details, operation name, method name, entity details (if provided), and the time elapsed since the timer started.
        /// </summary>
        /// <param name="ex">The exception object that was caught.</param>
        /// <param name="operation">A descriptive name of the operation that failed.</param>
        /// <param name="methode">The name of the method where the error occurred.</param>
        /// <param name="entity">The entity or data object related to the failed operation, or <see langword="null"/>.</param>
        public void LogError(Exception ex, string operation, string methode, object? entity)
        {
            if(entity == null)
            {
                Serilog.Log.Error(ex ,$"Error: {operation} failed in Method:{methode} after {_stopwatch.ElapsedMilliseconds} ms.");
                return;
            }
            Serilog.Log.Error(ex ,$"Error: {operation} failed on Entity: '{entity.ToString()}' in Method:{methode} after {_stopwatch.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Logs a simple, general informational message using Serilog's Information level.
        /// </summary>
        /// <param name="logText">The informational text to be logged.</param>
        public void LogInformationText(string logText)
        {
            Serilog.Log.Information(logText);
        }

        /// <summary>
        /// Stops and resets the internal <see cref="Stopwatch"/>. 
        /// Note: The elapsed time is logged in <see cref="LogSuccess"/> and <see cref="LogError"/> before this method is typically called.
        /// </summary>
        public void StopTimer()
        {
            _stopwatch.Stop();
            _stopwatch.Reset();
        }
    }
}
