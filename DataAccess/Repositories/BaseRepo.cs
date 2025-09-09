using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Serilog;
using LoggingService;

namespace DataAccess.Repositories
{
    /// <summary>
    /// Representes the base class for the repositories.
    /// </summary>
    /// <value>
    /// defines the constructor an the SaveChanges methode for every repository.
    /// </value>
    /// <param name="context">A DbContext object for the administration of the database.</param>
    public abstract class BaseRepo(ApiContext context, ILoggingService logger)
    {
        /// <summary>
        /// A DbContext object for the administration of the database.
        /// </summary>
        protected readonly ApiContext _context = context;
        protected readonly ILoggingService _logger = logger;

        /// <summary>
        /// Saves the changes in the data base.
        /// </summary>
        public async Task SaveChanges()
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

        public static void LogOperationTime(Stopwatch stopwatch, string entityName, string operation, object entity)
        {
            stopwatch.Stop();
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            string operationNameWithEntity = $"{entityName} {entity} {operation}";

            Log.Information($"{operationNameWithEntity} successfully in {elapsedMilliseconds} ms.");
        }

        protected async Task<T?> TryExecuteAsync<T>(Func<Task<T>> operation, string context)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _logger.Log("Error", context, ex.Message);
                return default;
            }
        }
    }
}
