using Xunit;

namespace FullingPorter.Core;

public sealed class PorterRulesTests
{
    [Fact]
    public void BlockedStatusOverridesReturningHome()
    {
        var status = PorterRules.GetStatusToken(
            PorterWorkState.ReturningHome,
            returningFromWork: true,
            blocked: true);

        Assert.Equal(PorterRules.BlockedStatus, status);
    }

    [Fact]
    public void ReturningWithoutWorkUsesIdleStatus()
    {
        var status = PorterRules.GetStatusToken(
            PorterWorkState.ReturningHome,
            returningFromWork: false,
            blocked: false);

        Assert.Equal(PorterRules.IdleStatus, status);
    }

    [Fact]
    public void ReturningAfterTripUsesReturningStatus()
    {
        var status = PorterRules.GetStatusToken(
            PorterWorkState.ReturningHome,
            returningFromWork: true,
            blocked: false);

        Assert.Equal(PorterRules.ReturningStatus, status);
    }

    [Theory]
    [InlineData((int)PorterWorkState.ToSource, PorterRules.CollectingStatus)]
    [InlineData((int)PorterWorkState.ToDestination, PorterRules.DeliveringStatus)]
    public void WorkingStatesExposeExpectedStatus(int stateValue, string expected)
    {
        var state = (PorterWorkState)stateValue;
        Assert.Equal(expected, PorterRules.GetStatusToken(state, false, false));
    }

    [Fact]
    public void BlockedDialogueOverridesPhysicalReturnState()
    {
        var context = PorterRules.GetDialogueContext(
            PorterWorkState.ReturningHome,
            returningFromWork: true,
            blocked: true);

        Assert.Equal(PorterDialogueContext.Blocked, context);
    }

    [Fact]
    public void ReturningWithoutWorkUsesIdleDialogue()
    {
        var context = PorterRules.GetDialogueContext(
            PorterWorkState.ReturningHome,
            returningFromWork: false,
            blocked: false);

        Assert.Equal(PorterDialogueContext.Idle, context);
    }

    [Theory]
    [InlineData((int)PorterWorkState.ToSource)]
    [InlineData((int)PorterWorkState.ToDestination)]
    public void TravelForWorkUsesWorkingDialogue(int stateValue)
    {
        var state = (PorterWorkState)stateValue;
        Assert.Equal(
            PorterDialogueContext.Working,
            PorterRules.GetDialogueContext(state, false, false));
    }

    [Theory]
    [InlineData(1, 3, true)]
    [InlineData(2, 3, true)]
    [InlineData(3, 3, false)]
    [InlineData(4, 3, false)]
    [InlineData(0, 3, false)]
    public void TransferRetryStopsAtConfiguredLimit(int failures, int maxFailures, bool expected)
    {
        Assert.Equal(expected, PorterRules.ShouldRetryTransfer(failures, maxFailures));
    }


    [Theory]
    [InlineData(100L, 100L, true)]
    [InlineData(100L, 101L, false)]
    [InlineData(0L, 0L, false)]
    public void RoutedSenderMustMatchActualPeer(long claimed, long actual, bool expected)
    {
        Assert.Equal(expected, PorterServerRequestRules.IsAuthenticatedSender(claimed, actual));
    }

[Theory]
    [InlineData(0f, 0f, 0f, 3f, 0f, 4f, true)]
    [InlineData(0f, 0f, 0f, 5f, 0f, 0f, true)]
    [InlineData(0f, 0f, 0f, 5.01f, 0f, 0f, false)]
    public void ServerInteractionDistanceIsBounded(
        float senderX,
        float senderY,
        float senderZ,
        float targetX,
        float targetY,
        float targetZ,
        bool expected)
    {
        Assert.Equal(
            expected,
            PorterServerRequestRules.IsWithinInteractionDistance(
                senderX,
                senderY,
                senderZ,
                targetX,
                targetY,
                targetZ));
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("Porter", true)]
    [InlineData("123456789012345678901234", true)]
    [InlineData("1234567890123456789012345", false)]
    public void ServerRenamePayloadHasHardLengthLimit(string name, bool expected)
    {
        Assert.Equal(expected, PorterServerRequestRules.IsRenamePayloadValid(name));
    }

    [Theory]
    [InlineData(10f, -1f, true)]
    [InlineData(10f, 9.8f, true)]
    [InlineData(10f, 9.9f, false)]
    public void ServerActionsAreRateLimited(float now, float lastActionAt, bool expected)
    {
        Assert.Equal(expected, PorterServerRequestRules.IsActionRateAllowed(now, lastActionAt));
    }

}
