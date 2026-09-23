using System.Collections.Generic;
using FullingPorter.Storage;
using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterWorker : MonoBehaviour
{
    private Character _character;
    private MonsterAI _ai;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _ai = GetComponent<MonsterAI>();

        // A porter must never acquire combat targets.
        if (_character != null)
            _character.m_faction = Character.Faction.Players;
    }

    private void Update()
    {
        if (_ai != null && _ai.GetTargetCreature() != null)
            _ai.SetTarget(null);
    }

    /// <summary>
    /// MVP routing primitive: returns Smart Storage containers accepting item.
    /// Movement, reservation and atomic transfer are implemented in the next milestone.
    /// </summary>
    internal IEnumerable<Container> FindDestinations(ItemDrop.ItemData item)
    {
        var radius = Plugin.WorkRadius.Value;
        foreach (var container in Container.GetAllContainers())
        {
            if (container == null) continue;
            if (Vector3.Distance(transform.position, container.transform.position) > radius) continue;
            if (QuickStackPlusBridge.Accepts(container, item))
                yield return container;
        }
    }
}
