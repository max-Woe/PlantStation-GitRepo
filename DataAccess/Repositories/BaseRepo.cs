using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;
using LoggingService;
using DataAccess.Interfaces;

namespace DataAccess.Repositories
{
    /// <summary>
    /// Representes the base class for the repositories.
    /// </summary>
    /// <value>
    /// defines the constructor an the SaveChanges methode for every repository.
    /// </value>
    /// <param name="context">A DbContext object for the administration of the database.</param>
    public abstract class BaseRepo(IApiContext context, ILoggingService logger)
    {
        /// <summary>
        /// A DbContext object for the administration of the database.
        /// </summary>
        protected readonly IApiContext _context = context;
        protected readonly ILoggingService _logger = logger;

        /// <summary>
        /// Saves the changes in the data base.
        /// </summary>
        public virtual async Task SaveChanges()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogDBError(ex);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogDBError(ex);
                throw;
            }
        }

        /// <summary>
        /// Attempts to execute the specified asynchronous operation and logs the outcome.
        /// </summary>
        /// <remarks>This method logs the execution time of the operation and records success or failure
        /// details. If the operation throws an exception, the exception is logged and rethrown.</remarks>
        /// <typeparam name="T">The type of the result returned by the operation.</typeparam>
        /// <param name="operation">The asynchronous operation to execute. Cannot be <see langword="null"/>.</param>
        /// <param name="operationName">The name of the operation, used for logging purposes. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="method">The name of the calling method, used for logging purposes. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="entity">An optional object associated with the operation, used for logging purposes. Can be <see langword="null"/>.</param>
        /// <returns>The result of the operation if it completes successfully; otherwise, <see langword="null"/>.</returns>
        protected async Task<T?> TryExecuteAsync<T>(Func<Task<T>> operation, string operationName, string method, object? entity = null)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                T? result = await operation();

                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                _logger.LogInformationText($"Abfragezeit: {String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10)}(HH:MM:SS:mSmS)");

                _logger.LogSuccess(operationName, method, entity);

                return result;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, operationName, method, entity);
             
                throw;
            }
        }
    }
}
