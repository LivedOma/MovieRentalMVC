using Serilog;
using ILogger = Serilog.ILogger;

namespace MovieRental.Services;

public class LoggerService : ILoggerService
{
    private readonly ILogger _logger;

    public LoggerService()
    {
        _logger = Log.ForContext<LoggerService>();
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.Information(message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.Warning(message, args);
    }

    public void LogError(string message, Exception? exception = null, params object[] args)
    {
        if (exception != null)
        {
            _logger.Error(exception, message, args);
        }
        else
        {
            _logger.Error(message, args);
        }
    }

    public void LogDebug(string message, params object[] args)
    {
        _logger.Debug(message, args);
    }

    public void LogUserAction(string userId, string action, string details)
    {
        _logger
            .ForContext("UserId", userId)
            .ForContext("Action", action)
            .Information("User Action: {Action} - {Details}", action, details);
    }

    public void LogSecurityEvent(string eventType, string details, string? userId = null)
    {
        var logger = _logger
            .ForContext("EventType", eventType)
            .ForContext("SecurityEvent", true);

        if (!string.IsNullOrEmpty(userId))
        {
            logger = logger.ForContext("UserId", userId);
        }

        logger.Warning("Security Event: {EventType} - {Details}", eventType, details);
    }

    public void LogDatabaseOperation(string operation, string entity, int? entityId = null)
    {
        var logger = _logger
            .ForContext("Operation", operation)
            .ForContext("Entity", entity);

        if (entityId.HasValue)
        {
            logger = logger.ForContext("EntityId", entityId.Value);
        }

        logger.Debug("Database: {Operation} on {Entity}", operation, entity);
    }
}