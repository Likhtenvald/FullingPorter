using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullingPorter.Storage;

/// <summary>
/// Compatibility bridge for QuickStackPlus 1.2.x Smart Storage.
/// The format is intentionally isolated here so changes in QuickStackPlus
/// do not leak into porter AI.
/// </summary>
internal static class QuickStackPlusBridge
{
    internal const string StorageFilterZdoKey = "Goneryx.QuickStackPlus.StorageFilter";
    internal const char Separator = '\u001f';

    internal static HashSet<string> GetAcceptedItemIds(Container container)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        if (container == null) return result;

        var view = container.GetComponent<ZNetView>();
        if (view == null || !view.IsValid()) return result;

        var zdo = view.GetZDO();
        if (zdo == null) return result;

        var raw = zdo.GetString(StorageFilterZdoKey, string.Empty);
        if (string.IsNullOrEmpty(raw)) return result;

        foreach (var id in raw.Split(new[] { Separator }, StringSplitOptions.RemoveEmptyEntries))
            result.Add(id);

        return result;
    }

    internal static bool Accepts(Container container, ItemDrop.ItemData item)
    {
        if (item?.m_dropPrefab == null) return false;
        return GetAcceptedItemIds(container).Contains(item.m_dropPrefab.name);
    }
}
