namespace FullingPorter.Core;

internal enum PorterWorkState
{
    Idle,
    ToSource,
    ToDestination,
    ReturningHome
}

internal enum PorterDialogueContext
{
    Idle,
    Working,
    Returning,
    Blocked
}

internal static class PorterRules
{
    internal const string IdleStatus = "$fullingporter_status_idle";
    internal const string CollectingStatus = "$fullingporter_status_collecting";
    internal const string DeliveringStatus = "$fullingporter_status_delivering";
    internal const string ReturningStatus = "$fullingporter_status_returning";
    internal const string BlockedStatus = "$fullingporter_status_blocked";

    internal static PorterDialogueContext GetDialogueContext(
        PorterWorkState state,
        bool returningFromWork,
        bool blocked)
    {
        if (blocked)
            return PorterDialogueContext.Blocked;

        switch (state)
        {
            case PorterWorkState.ToSource:
            case PorterWorkState.ToDestination:
                return PorterDialogueContext.Working;
            case PorterWorkState.ReturningHome:
                return returningFromWork
                    ? PorterDialogueContext.Returning
                    : PorterDialogueContext.Idle;
            default:
                return PorterDialogueContext.Idle;
        }
    }

    internal static string GetStatusToken(
        PorterWorkState state,
        bool returningFromWork,
        bool blocked)
    {
        if (blocked)
            return BlockedStatus;

        switch (state)
        {
            case PorterWorkState.ToSource:
                return CollectingStatus;
            case PorterWorkState.ToDestination:
                return DeliveringStatus;
            case PorterWorkState.ReturningHome:
                return returningFromWork ? ReturningStatus : IdleStatus;
            default:
                return IdleStatus;
        }
    }

    internal static bool ShouldRetryTransfer(int failureCount, int maxFailures)
    {
        return failureCount > 0 && maxFailures > 0 && failureCount < maxFailures;
    }
}
