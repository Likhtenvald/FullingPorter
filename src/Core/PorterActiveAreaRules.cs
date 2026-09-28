namespace FullingPorter.Core;

internal static class PorterActiveAreaRules
{
    internal static bool NeedsServerArea(
        float homeX,
        float homeZ,
        float hostX,
        float hostZ,
        float activeRadius)
    {
        if (activeRadius <= 0f)
            return false;

        var dx = hostX - homeX;
        var dz = hostZ - homeZ;
        return dx * dx + dz * dz > activeRadius * activeRadius;
    }
}
