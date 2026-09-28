using Xunit;

namespace FullingPorter.Core;

public sealed class PorterWaitTrackerTests
{
    private const float PlayerTimeout = 30f;
    private const float LockTimeout = 5f;
    private const float OwnershipTimeout = 3f;

    [Fact]
    public void ClosingPlayerHeldChestAllowsDeliveryAndNextTrip()
    {
        var wait = new PorterWaitTracker();
        var firstStack = new object();

        Assert.Equal(PorterWaitDecision.Started, wait.PlayerBusy(0f, PlayerTimeout));
        Assert.Equal(PorterWaitDecision.Waiting, wait.PlayerBusy(15f, PlayerTimeout));

        wait.PlayerAvailable();
        Assert.Equal(
            PorterWaitDecision.Started,
            wait.OwnershipPending(firstStack, 15.1f, OwnershipTimeout, LockTimeout));
        Assert.Equal(
            PorterWaitDecision.Waiting,
            wait.OwnershipPending(firstStack, 15.3f, OwnershipTimeout, LockTimeout));

        wait.Reset(); // The first transfer and trip complete.
        Assert.Equal(
            PorterWaitDecision.Started,
            wait.OwnershipPending(new object(), 20f, OwnershipTimeout, LockTimeout));
    }

    [Fact]
    public void PlayerHeldChestTimesOutButNewTripStartsFresh()
    {
        var wait = new PorterWaitTracker();

        Assert.Equal(PorterWaitDecision.Started, wait.PlayerBusy(10f, PlayerTimeout));
        Assert.Equal(PorterWaitDecision.Waiting, wait.PlayerBusy(39.9f, PlayerTimeout));
        Assert.Equal(PorterWaitDecision.TimedOut, wait.PlayerBusy(40f, PlayerTimeout));

        wait.Reset(); // AbortBatch/ClearBatch.
        Assert.Equal(PorterWaitDecision.Started, wait.PlayerBusy(45f, PlayerTimeout));
    }

    [Fact]
    public void AlternatingLockAndOwnershipRetriesCannotExtendLockDeadline()
    {
        var wait = new PorterWaitTracker();
        var stack = new object();

        Assert.Equal(PorterWaitDecision.Started, wait.LockBusy(0f, LockTimeout));
        Assert.Equal(
            PorterWaitDecision.Started,
            wait.OwnershipPending(stack, 0.1f, OwnershipTimeout, LockTimeout));
        Assert.Equal(PorterWaitDecision.Waiting, wait.LockBusy(2f, LockTimeout));
        Assert.Equal(
            PorterWaitDecision.Started,
            wait.OwnershipPending(stack, 2.1f, OwnershipTimeout, LockTimeout));
        Assert.Equal(
            PorterWaitDecision.LockTimedOut,
            wait.OwnershipPending(stack, 5f, OwnershipTimeout, LockTimeout));
    }

    [Fact]
    public void PlayerAccessClearsEarlierPorterLockDeadline()
    {
        var wait = new PorterWaitTracker();
        var stack = new object();

        Assert.Equal(PorterWaitDecision.Started, wait.LockBusy(0f, LockTimeout));
        Assert.Equal(PorterWaitDecision.Started, wait.PlayerBusy(2f, PlayerTimeout));
        wait.PlayerAvailable();

        Assert.Equal(
            PorterWaitDecision.Started,
            wait.OwnershipPending(stack, 12f, OwnershipTimeout, LockTimeout));
    }

    [Fact]
    public void OwnershipTimeoutSkipsOnlyCurrentStack()
    {
        var wait = new PorterWaitTracker();
        var firstStack = new object();

        Assert.Equal(
            PorterWaitDecision.Started,
            wait.OwnershipPending(firstStack, 0f, OwnershipTimeout, LockTimeout));
        Assert.Equal(
            PorterWaitDecision.Waiting,
            wait.OwnershipPending(firstStack, 2.9f, OwnershipTimeout, LockTimeout));
        Assert.Equal(
            PorterWaitDecision.TimedOut,
            wait.OwnershipPending(firstStack, 3f, OwnershipTimeout, LockTimeout));

        wait.Reset(); // RemoveCargoEntryAndContinue.
        Assert.Equal(
            PorterWaitDecision.Started,
            wait.OwnershipPending(new object(), 3.1f, OwnershipTimeout, LockTimeout));
    }
}
