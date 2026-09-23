using UnityEngine;

namespace FullingPorter.Storage;

internal static class SourceContainerMarker
{
    internal const string SourceKey = "FullingPorter.Source";

    internal static bool IsSource(Container container)
    {
        var view = container ? container.GetComponent<ZNetView>() : null;
        return view != null && view.IsValid() && view.GetZDO()?.GetBool(SourceKey, false) == true;
    }

    internal static bool Toggle(Container container)
    {
        var view = container ? container.GetComponent<ZNetView>() : null;
        if (view == null || !view.IsValid()) return false;
        if (!view.IsOwner()) view.ClaimOwnership();
        var zdo = view.GetZDO();
        var value = !zdo.GetBool(SourceKey, false);
        zdo.Set(SourceKey, value);
        return value;
    }
}
