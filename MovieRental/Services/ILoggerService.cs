namespace MovieRental.Services;

public interface ILoggerService
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(string message, Exception? exception = null, params object[] args);
    void LogDebug(string message, params object[] args);
    
    // Métodos específicos de la aplicación
    void LogUserAction(string userId, string action, string details);
    void LogSecurityEvent(string eventType, string details, string? userId = null);
    void LogDatabaseOperation(string operation, string entity, int? entityId = null);
}