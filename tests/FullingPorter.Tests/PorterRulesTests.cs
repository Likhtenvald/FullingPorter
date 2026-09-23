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
    [InlineData(PorterWorkState.ToSource, PorterRules.CollectingStatus)]
    [InlineData(PorterWorkState.ToDestination, PorterRules.DeliveringStatus)]
    public void WorkingStatesExposeExpectedStatus(PorterWorkState state, string expected)
    {
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
    [InlineData(PorterWorkState.ToSource)]
    [InlineData(PorterWorkState.ToDestination)]
    public void TravelForWorkUsesWorkingDialogue(PorterWorkState state)
    {
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
}
