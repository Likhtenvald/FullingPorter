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

    internal static bool TryToggleServer(Container container, out bool enabled)
    {
        enabled = false;
        if (ZNet.instance == null || !ZNet.instance.IsServer())
            return false;

        var view = container ? container.GetComponent<ZNetView>() : null;
        if (view == null || !view.IsValid())
            return false;

        var zdo = view.GetZDO();
        if (zdo == null)
            return false;

        enabled = !zdo.GetBool(SourceKey, false);
        zdo.Set(SourceKey, enabled);
        return true;
    }

}
