namespace LoggingService
{
    public interface ILoggingService
    {
        void StartTimer();
        void Log(string entityName, string operation, object entity);
        void StopTimer();
        void LogDBError(Exception ex);
    }
}