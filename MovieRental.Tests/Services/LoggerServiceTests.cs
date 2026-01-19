using FluentAssertions;
using MovieRental.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;

namespace MovieRental.Tests.Services;

public class LoggerServiceTests : IDisposable
{
    private readonly ILoggerService _loggerService;

    public LoggerServiceTests()
    {
        // Configurar Serilog para testing
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestCorrelator()
            .CreateLogger();

        _loggerService = new LoggerService();
    }

    [Fact]
    public void LogInformation_ShouldLogMessage()
    {
        // Arrange
        using (TestCorrelator.CreateContext())
        {
            // Act
            _loggerService.LogInformation("Test message {Param}", "value");

            // Assert
            var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
            logEvents.Should().ContainSingle();
            logEvents.First().Level.Should().Be(LogEventLevel.Information);
        }
    }

    [Fact]
    public void LogWarning_ShouldLogMessage()
    {
        // Arrange
        using (TestCorrelator.CreateContext())
        {
            // Act
            _loggerService.LogWarning("Warning message");

            // Assert
            var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
            logEvents.Should().ContainSingle();
            logEvents.First().Level.Should().Be(LogEventLevel.Warning);
        }
    }

    [Fact]
    public void LogError_WithException_ShouldLogMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");

        using (TestCorrelator.CreateContext())
        {
            // Act
            _loggerService.LogError("Error occurred", exception);

            // Assert
            var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
            logEvents.Should().ContainSingle();
            logEvents.First().Level.Should().Be(LogEventLevel.Error);
            logEvents.First().Exception.Should().Be(exception);
        }
    }

    [Fact]
    public void LogUserAction_ShouldIncludeUserIdAndAction()
    {
        // Arrange
        using (TestCorrelator.CreateContext())
        {
            // Act
            _loggerService.LogUserAction("user123", "Login", "User logged in successfully");

            // Assert
            var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
            logEvents.Should().ContainSingle();
            
            var logEvent = logEvents.First();
            logEvent.Level.Should().Be(LogEventLevel.Information);
            logEvent.Properties.Should().ContainKey("UserId");
            logEvent.Properties.Should().ContainKey("Action");
        }
    }

    [Fact]
    public void LogSecurityEvent_ShouldIncludeSecurityFlag()
    {
        // Arrange
        using (TestCorrelator.CreateContext())
        {
            // Act
            _loggerService.LogSecurityEvent("LoginFailed", "Failed login attempt", "user123");

            // Assert
            var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
            logEvents.Should().ContainSingle();
            
            var logEvent = logEvents.First();
            logEvent.Level.Should().Be(LogEventLevel.Warning);
            logEvent.Properties.Should().ContainKey("SecurityEvent");
            logEvent.Properties.Should().ContainKey("EventType");
        }
    }

    [Fact]
    public void LogDatabaseOperation_ShouldIncludeOperationDetails()
    {
        // Arrange
        using (TestCorrelator.CreateContext())
        {
            // Act
            _loggerService.LogDatabaseOperation("Create", "Movie", 123);

            // Assert
            var logEvents = TestCorrelator.GetLogEventsFromCurrentContext();
            logEvents.Should().ContainSingle();
            
            var logEvent = logEvents.First();
            logEvent.Level.Should().Be(LogEventLevel.Debug);
            logEvent.Properties.Should().ContainKey("Operation");
            logEvent.Properties.Should().ContainKey("Entity");
            logEvent.Properties.Should().ContainKey("EntityId");
        }
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
    }
}