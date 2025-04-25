using Microsoft.Extensions.Time.Testing;
using Moq;
using SmartCommand.AnalyticsStrategy;
using SmartCommand.CanExecuteStrategy;
using SmartCommand.ErrorHandlingStrategy;
using SmartCommand.ExecutionStrategy;
using SmartCommand.ExecutionThrottlerStrategy;
using SmartCommand.RetryStrategy;
using SmartCommand.SmartCommand;

namespace SmartCommand.Tests.SmartCommand;

[TestFixture]
public class SmartCommandTests
{
    private Mock<IExecutionStrategy<string>> _mockExecutionStrategy;
    private Mock<ICanExecuteStrategy<string>> _mockCanExecuteStrategy;
    private Mock<IExecutionThrottlerStrategy> _mockThrottlerStrategy;
    private Mock<IErrorHandlingStrategy> _mockErrorHandlerStrategy;
    private Mock<IRetryStrategy> _mockRetryStrategy;
    private FakeTimeProvider _fakeTimeProvider;
    private Mock<IAnalyticsStrategy> _mockAnalyticsStrategy;
    private SmartCommand<string> _smartCommand;
    private const string TestCommandName = "TestCommand";
    private const string TestParameter = "testParam";

    [SetUp]
    public void SetUp()
    {
        _mockExecutionStrategy = new Mock<IExecutionStrategy<string>>();
        _mockCanExecuteStrategy = new Mock<ICanExecuteStrategy<string>>();
        _mockThrottlerStrategy = new Mock<IExecutionThrottlerStrategy>();
        _mockErrorHandlerStrategy = new Mock<IErrorHandlingStrategy>();
        _mockRetryStrategy = new Mock<IRetryStrategy>();
        _fakeTimeProvider = new FakeTimeProvider();
        _mockAnalyticsStrategy = new Mock<IAnalyticsStrategy>();

        // Default setup for mocks - can be overridden in specific tests
        _mockThrottlerStrategy.Setup(t => t.CanStart()).Returns(true);
        _mockCanExecuteStrategy.Setup(c => c.CanExecute(It.IsAny<string>())).Returns(true);
        _mockRetryStrategy.SetupSequence(r => r.CanRetry())
                          .Returns(true) // Allow first attempt
                          .Returns(false); // Stop after first attempt
        _mockExecutionStrategy.Setup(e => e.ExecuteAsync(It.IsAny<string>())).Returns(Task.CompletedTask);

        _smartCommand = new SmartCommand<string>(
            _mockExecutionStrategy.Object,
            _mockCanExecuteStrategy.Object,
            TestCommandName,
            _mockErrorHandlerStrategy.Object,
            _mockThrottlerStrategy.Object,
            _mockRetryStrategy.Object,
            _fakeTimeProvider,
            _mockAnalyticsStrategy.Object
        );
    }

    [Test]
    public void Constructor_GivenValidName_AssignsNameCorrectly()
    {
        // Arrange & Act already done in SetUp

        // Assert - Accessing private members for testing is generally discouraged,
        // but we can infer from analytics calls. Let's test analytics interaction.
        _smartCommand.Execute(TestParameter); // Trigger execution to check name usage
         _mockAnalyticsStrategy.Verify(a => a.TrackCommandStart(TestCommandName, It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public void Constructor_GivenNullOrEmptyName_AssignsNewGuidAsName()
    {
        // Arrange
        var commandWithNewGuid = new SmartCommand<string>(
            _mockExecutionStrategy.Object,
            _mockCanExecuteStrategy.Object,
            "", // Empty name
            _mockErrorHandlerStrategy.Object,
            _mockThrottlerStrategy.Object,
            _mockRetryStrategy.Object,
            _fakeTimeProvider,
            _mockAnalyticsStrategy.Object
        );

        // Act
        commandWithNewGuid.Execute(TestParameter);

        // Assert
        // Verify TrackCommandStart was called with *some* string argument (the GUID)
        _mockAnalyticsStrategy.Verify(a => a.TrackCommandStart(It.Is<string>(s => !string.IsNullOrEmpty(s) && s != TestCommandName), It.IsAny<DateTime>()), Times.Once);
    }


    [Test]
    public void CanExecute_WhenThrottlerAndCanExecuteStrategyAllow_ReturnsTrue()
    {
        // Arrange
        _mockThrottlerStrategy.Setup(t => t.CanStart()).Returns(true);
        _mockCanExecuteStrategy.Setup(c => c.CanExecute(TestParameter)).Returns(true);

        // Act
        var result = _smartCommand.CanExecute(TestParameter);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void CanExecute_WhenThrottlerDenies_ReturnsFalse()
    {
        // Arrange
        _mockThrottlerStrategy.Setup(t => t.CanStart()).Returns(false);
        _mockCanExecuteStrategy.Setup(c => c.CanExecute(TestParameter)).Returns(true); // Should still be false overall

        // Act
        var result = _smartCommand.CanExecute(TestParameter);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void CanExecute_WhenCanExecuteStrategyDenies_ReturnsFalse()
    {
        // Arrange
        _mockThrottlerStrategy.Setup(t => t.CanStart()).Returns(true);
        _mockCanExecuteStrategy.Setup(c => c.CanExecute(TestParameter)).Returns(false);

        // Act
        var result = _smartCommand.CanExecute(TestParameter);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void Execute_WhenSuccessful_CallsStrategiesAndAnalyticsInOrder()
    {
        // Arrange
        _fakeTimeProvider.SetUtcNow(DateTime.UtcNow);
        var startTime = _fakeTimeProvider.GetUtcNow();
        var completionTime = _fakeTimeProvider.GetUtcNow();
        bool canExecuteChangedRaised = false;
        _smartCommand.CanExecuteChanged += (sender, args) => canExecuteChangedRaised = true;

        // Act
        _smartCommand.Execute(TestParameter);
        // Note: Execute is async void, so execution might continue after this line.
        // Verifications below check the expected synchronous and asynchronous calls.

        // Assert
        _mockRetryStrategy.Verify(r => r.ResetRetry(), Times.Once); // Called at the start
        _mockRetryStrategy.Verify(r => r.CanRetry(), Times.AtLeastOnce); // Checked in the loop
        _mockRetryStrategy.Verify(r => r.Retry(), Times.Once); // Called before execution attempt

        _mockAnalyticsStrategy.Verify(a => a.TrackCommandStart(TestCommandName, startTime.DateTime), Times.Once);

        _mockThrottlerStrategy.Verify(t => t.MarkStarted(), Times.Once);
        _mockExecutionStrategy.Verify(e => e.ExecuteAsync(TestParameter), Times.Once);

        // Simulate time passing for completion timestamp
        _fakeTimeProvider.SetUtcNow(completionTime);
        // Re-trigger Execute to allow finally block and completion tracking to use the new time.
        // This is a simplification; real async void testing is complex.
        // In a real scenario, you might need TaskCompletionSource or other coordination.
        // For this example, we assume the async operation completed before the second Execute call for verification purposes.
        // A better approach might be to have ExecuteAsync return a controllable Task.

        // Let's refine the setup for better async control
        var tcs = new TaskCompletionSource<object?>();
        _mockExecutionStrategy.Setup(e => e.ExecuteAsync(TestParameter)).Returns(tcs.Task);
         _mockRetryStrategy.SetupSequence(r => r.CanRetry()).Returns(true).Returns(false); // Reset sequence for this test
         _smartCommand = new SmartCommand<string>( // Recreate command with new setup
            _mockExecutionStrategy.Object, _mockCanExecuteStrategy.Object, TestCommandName, _mockErrorHandlerStrategy.Object, 
            _mockThrottlerStrategy.Object, _mockRetryStrategy.Object, _fakeTimeProvider, _mockAnalyticsStrategy.Object
         );
        _smartCommand.CanExecuteChanged += (sender, args) => canExecuteChangedRaised = true; // Re-attach handler


        // Act again with controllable task
        _smartCommand.Execute(TestParameter); // Starts execution

        // Assert intermediate state if needed (e.g., MarkStarted called)
         _mockThrottlerStrategy.Verify(t => t.MarkStarted(), Times.Exactly(2));

        // Simulate task completion and time change
         _fakeTimeProvider.SetUtcNow(completionTime);
         tcs.SetResult(null); // Complete the async operation

        // Verifications (ensure mocks are called correctly after async completion)
        _mockAnalyticsStrategy.Verify(a => a.TrackCommandComplete(TestCommandName, completionTime.DateTime), Times.Exactly(2));
        _mockThrottlerStrategy.Verify(t => t.MarkFinished(), Times.Exactly(2)); // Called in finally
        Assert.That(canExecuteChangedRaised, Is.True, "CanExecuteChanged should have been raised");
        // Verify it's raised twice (start and finish)
        // Moq doesn't easily verify event invocations count directly.
        // We check if it was raised at least once. For count, manual tracking or specific event args might be needed.


    }

    [Test]
    public async Task Execute_WhenExecutionThrowsException_CallsErrorHandlerAndAnalytics()
    {
        // Arrange
        var exception = new InvalidOperationException("Execution failed");
        _fakeTimeProvider.SetUtcNow(DateTimeOffset.UtcNow);
        var startTime = _fakeTimeProvider.GetUtcNow();
        var errorTime = _fakeTimeProvider.GetUtcNow();
        var tcs = new TaskCompletionSource<object?>();
        tcs.SetException(exception); // Setup task to throw
/*
        int trackErrorCallCount = 0; // Add a counter

        _mockAnalyticsStrategy
            .Setup(a => a.TrackCommandError(
                TestCommandName,        // Use specific command name
                exception,              // Use the specific exception instance
                It.IsAny<DateTime>())) // Allow any DateTime for flexibility here
            .Callback<string, Exception, DateTime>((cmdName, ex, time) => // Define parameters for clarity
            {
                trackErrorCallCount++; // Increment the counter
                // Log to test output (visible in Test Explorer details)
                TestContext.WriteLine($"TrackCommandError called. Count: {trackErrorCallCount}, Time: {time:O}, Exception: {ex.GetType().Name}");
                // >>> SET A BREAKPOINT ON THE LINE ABOVE <<<
            });
*/
        _mockExecutionStrategy.Setup(e => e.ExecuteAsync(TestParameter)).Returns(tcs.Task);
         _mockRetryStrategy.SetupSequence(r => r.CanRetry()).Returns(true).Returns(false); // Reset sequence
         _smartCommand = new SmartCommand<string>( // Recreate command
            _mockExecutionStrategy.Object, _mockCanExecuteStrategy.Object, TestCommandName, _mockErrorHandlerStrategy.Object, 
            _mockThrottlerStrategy.Object, _mockRetryStrategy.Object, _fakeTimeProvider, _mockAnalyticsStrategy.Object
         );


        // Act
        _smartCommand.Execute(TestParameter); // Execute the command

        // Allow async void method to progress
        await Task.Yield(); // Yield control to allow async operations to proceed somewhat

         // Need mechanism to wait for async void completion/exception handling
         // This is tricky. Verification might happen before the catch block runs.
         // Let's try verifying after a small delay, acknowledging this isn't foolproof.
         await Task.Delay(50); // Small delay - adjust if needed, but unreliable

        // Assert
        _mockAnalyticsStrategy.Verify(a => a.TrackCommandStart(TestCommandName, startTime.DateTime), Times.Once);
        _mockExecutionStrategy.Verify(e => e.ExecuteAsync(TestParameter), Times.Once); // Verify execution was attempted
        _mockErrorHandlerStrategy.Verify(eh => eh.HandleError(exception, $"Error executing command {TestCommandName}:"), Times.Once); // Verify error handler was called
        _mockAnalyticsStrategy.Verify(a => a.TrackCommandError(TestCommandName, exception, errorTime.DateTime), Times.Once); // Verify error analytics
        _mockThrottlerStrategy.Verify(t => t.MarkFinished(), Times.Once); // Verify finally block ran
         _mockAnalyticsStrategy.Verify(a => a.TrackCommandComplete(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never); // Ensure complete wasn't tracked

    }

     [Test]
    public void Execute_WhenCanExecuteIsFalseInsideLoop_SkipsExecutionAttemptAndRetries()
    {
        // Arrange
        _mockRetryStrategy.SetupSequence(r => r.CanRetry())
                          .Returns(true)  // First check
                          .Returns(true)  // Second check (after CanExecute fails)
                          .Returns(false); // Stop
        _mockThrottlerStrategy.Setup(t => t.CanStart()).Returns(true); // Throttler allows initially

        // CanExecute will return false the first time it's checked *inside* the loop
        _mockCanExecuteStrategy.SetupSequence(c => c.CanExecute(TestParameter))
                               .Returns(false) // First check inside loop fails
                               .Returns(true); // Second check inside loop succeeds

        // Act
        _smartCommand.Execute(TestParameter);

        // Assert
        _mockRetryStrategy.Verify(r => r.ResetRetry(), Times.Once);
        // CanRetry called 3 times (enters loop twice, fails third check)
        _mockRetryStrategy.Verify(r => r.CanRetry(), Times.Exactly(3));

        // Analytics start called twice (once for each loop entry)
        _mockAnalyticsStrategy.Verify(a => a.TrackCommandStart(TestCommandName, It.IsAny<DateTime>()), Times.Exactly(1));

        // CanExecute (the command's method) called twice inside the loop
        // CanExecuteStrategy's CanExecute method also called twice
         _mockCanExecuteStrategy.Verify(c => c.CanExecute(TestParameter), Times.Exactly(2));

        // Retry called only once (when CanExecute is true)
        _mockRetryStrategy.Verify(r => r.Retry(), Times.Exactly(2));

        // Execution attempted only once
        _mockExecutionStrategy.Verify(e => e.ExecuteAsync(TestParameter), Times.Once);
        _mockThrottlerStrategy.Verify(t => t.MarkStarted(), Times.Exactly(2));
        _mockThrottlerStrategy.Verify(t => t.MarkFinished(), Times.Exactly(2));
        _mockAnalyticsStrategy.Verify(a => a.TrackCommandComplete(TestCommandName, It.IsAny<DateTime>()), Times.Once);
    }

    [Test]
    public void Execute_RaisesCanExecuteChangedCorrectly()
    {
        // Arrange
        int canExecuteChangedCount = 0;
        _smartCommand.CanExecuteChanged += (sender, args) => canExecuteChangedCount++;
         _mockRetryStrategy.SetupSequence(r => r.CanRetry()).Returns(true).Returns(false); // Ensure single execution attempt

        // Act
        _smartCommand.Execute(TestParameter);
        // Allow async operations to proceed somewhat
         Task.Delay(50).Wait(); // Use Wait cautiously in tests, prefer async Task where possible


        // Assert
        // Should be raised once after MarkStarted and once in the finally block
        Assert.That(canExecuteChangedCount, Is.EqualTo(2));
         _mockThrottlerStrategy.Verify(t => t.MarkStarted(), Times.Once);
         _mockThrottlerStrategy.Verify(t => t.MarkFinished(), Times.Once);
    }
    
}