namespace FullingPorter.Core;

internal enum PorterServerAction
{
    Spawn,
    ToggleSource,
    Dismiss,
    Rename
}

internal static class PorterServerRequestRules
{
    internal const float InteractionDistance = 5f;
    internal const float InteractionDistanceSqr = InteractionDistance * InteractionDistance;
    internal const int MaxRenameLength = 24;
    internal const float MinActionIntervalSeconds = 0.15f;

    internal static bool IsAuthenticatedSender(long claimedSender, long actualPeerUid)
    {
        return claimedSender != 0L && claimedSender == actualPeerUid;
    }

    internal static bool IsWithinInteractionDistance(
        float senderX,
        float senderY,
        float senderZ,
        float targetX,
        float targetY,
        float targetZ)
    {
        var dx = senderX - targetX;
        var dy = senderY - targetY;
        var dz = senderZ - targetZ;
        return dx * dx + dy * dy + dz * dz <= InteractionDistanceSqr;
    }

    internal static bool IsRenamePayloadValid(string name)
    {
        if (name == null)
            return false;

        return name.Trim().Length <= MaxRenameLength;
    }

    internal static bool IsActionRateAllowed(float now, float lastActionAt)
    {
        if (lastActionAt < 0f)
            return true;

        return now - lastActionAt >= MinActionIntervalSeconds;
    }
}
