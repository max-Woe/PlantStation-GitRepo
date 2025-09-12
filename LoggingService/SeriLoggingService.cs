using System.Diagnostics;
using Serilog;

namespace LoggingService
{
    public class SeriLoggingService : ILoggingService
    {
        Stopwatch _stopwatch = new Stopwatch();
        public void LogDBError(Exception ex)
        {
            Serilog.Log.Error(ex, ex.Message);
        }


        public void StartTimer()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public void Log(string entityName, string operation, object entity)
        {
            long elapsedMilliseconds = _stopwatch.ElapsedMilliseconds;

            string operationNameWithEntity = $"{entityName} {entity} {operation}";

            Serilog.Log.Information($"{operationNameWithEntity} successfully in {elapsedMilliseconds} ms.");
        }
        
        public void StopTimer()
        {
            _stopwatch.Stop();
        }
    }
}
