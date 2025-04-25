using Microsoft.Extensions.Logging;
using Moq;
using SmartCommand.AnalyticsStrategy;

namespace SmartCommand.Tests.AnalyticsStrategy;

[TestFixture]
public class LogAnalyticsStrategyTests
{
    private Mock<ILogger> _loggerMock;
    private LogAnalyticsStrategy _strategy;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger>();
        _strategy = new LogAnalyticsStrategy(_loggerMock.Object, LogLevel.Information);
    }

    [Test]
    public void TrackCommandStart_CallsLoggerWithCorrectParameters()
    {
        // Arrange
        const string commandName = "TestStartCommand";
        var startTime = DateTime.UtcNow;

        // Act
        _strategy.TrackCommandStart(commandName, startTime);

        // Assert: Verify LogInformation was called correctly by checking the underlying Log method
        _loggerMock.Verify(
            logger => logger.Log(
                // Check the LogLevel
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                // Check the EventId (usually not critical for this type of logging)
                It.IsAny<EventId>(),
                // Check the state object (structured log data)
                It.Is<It.IsAnyType>((state, type) => VerifyLogState(
                    state,
                    "Command {CommandName} started at {StartTime}",
                    new KeyValuePair<string, object>("CommandName", commandName),
                    new KeyValuePair<string, object>("StartTime", startTime))),
                // Check that no exception was passed
                It.Is<Exception>(exception => exception == null),
                // Check the formatter (Moq matcher for any function)
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            // Ensure it was called exactly once
            Times.Once);
    }

    [Test]
    public void TrackCommandComplete_CallsLoggerWithCorrectParameters()
    {
        // Arrange
        const string commandName = "TestCompleteCommand";
        var endTime = DateTime.UtcNow;

        // Act
        _strategy.TrackCommandComplete(commandName, endTime);

        // Assert: Verify LogInformation was called correctly by checking the underlying Log method
        _loggerMock.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) => VerifyLogState(
                    state,
                    "Command {CommandName} stopped at {StartTime}", // Note: Template uses StartTime key from method
                    new KeyValuePair<string, object>("CommandName", commandName),
                    new KeyValuePair<string, object>("StartTime", endTime))), // Value is endTime
                It.Is<Exception>(exception => exception == null),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public void TrackCommandError_CallsLoggerWithCorrectParametersAndException()
    {
        // Arrange
        const string commandName = "TestErrorCommand";
        var errorTime = DateTime.UtcNow;
        var exception = new InvalidOperationException("Something went wrong");

        // Act
        _strategy.TrackCommandError(commandName, exception, errorTime);

        // Assert: Verify Log was called correctly
        _loggerMock.Verify(
            logger => logger.Log(
                // Check the LogLevel
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                // Check the EventId
                It.IsAny<EventId>(),
                // Check the state object
                It.Is<It.IsAnyType>((state, type) => VerifyLogState(
                    state,
                    "Command {CommandName} stopped at {ErrorTime}",
                    new KeyValuePair<string, object>("CommandName", commandName),
                    new KeyValuePair<string, object>("ErrorTime", errorTime))),
                // Check that the correct exception was passed
                It.Is<Exception>(ex => ex == exception),
                // Check the formatter
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Helper method to verify the structured log state.
    /// </summary>
    private static bool VerifyLogState(object state, string expectedTemplate, params KeyValuePair<string, object>[] expectedParams)
    {
        // The state object for structured logging is often a collection of key-value pairs.
        var stateDict = state as IReadOnlyList<KeyValuePair<string, object>>;
        if (stateDict == null)
        {
            // If it's not the expected type, the verification fails.
            TestContext.WriteLine($"State was not IReadOnlyList<KeyValuePair<string, object>>. Actual type: {state?.GetType().Name ?? "null"}");
            return false;
        }

        // Convert to a dictionary for easier lookup. Include the {OriginalFormat} key which holds the template.
        var stateLookup = stateDict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // 1. Verify the message template ({OriginalFormat})
        if (!stateLookup.TryGetValue("{OriginalFormat}", out var actualTemplate) || actualTemplate?.ToString() != expectedTemplate)
        {
            TestContext.WriteLine($"Template mismatch. Expected: '{expectedTemplate}', Actual: '{actualTemplate}'");
            return false;
        }

        // 2. Verify all expected parameters are present and have the correct values
        foreach (var kvp in expectedParams)
        {
            if (!stateLookup.TryGetValue(kvp.Key, out var actualValue) || !Equals(actualValue, kvp.Value))
            {
                TestContext.WriteLine($"Parameter mismatch for key '{kvp.Key}'. Expected: '{kvp.Value}', Actual: '{actualValue}'");
                return false;
            }
        }

        // Optional: Check if there are unexpected parameters (adjust count check as needed)
        // The count should be expectedParams + 1 (for {OriginalFormat})
        if (stateLookup.Count != expectedParams.Length + 1)
        {
            TestContext.WriteLine($"Parameter count mismatch. Expected: {expectedParams.Length + 1}, Actual: {stateLookup.Count}");
            // Depending on strictness, you might return false here or just log it.
            // return false;
        }
        return true; // All checks passed
    }
}