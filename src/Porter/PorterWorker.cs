using System.Collections.Generic;
using FullingPorter.Storage;
using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterWorker : MonoBehaviour
{
    private enum WorkState { Idle, ToSource, ToDestination, ReturningHome }

    private sealed class CargoEntry
    {
        internal ItemDrop.ItemData Item;
        internal Container Destination;
    }

    private Character _character;
    private MonsterAI _ai;
    private ZNetView _view;
    private WorkState _state;
    private Container _source;
    private readonly List<CargoEntry> _cargo = new();
    private Vector3 _home;
    private float _nextScan;

    private const float InteractionDistance = 2.2f;
    private const float ScanInterval = 2f;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _ai = GetComponent<MonsterAI>();
        _view = GetComponent<ZNetView>();
        _home = transform.position;
        if (_character != null) _character.m_faction = Character.Faction.Players;
    }

    private void Update()
    {
        if (_view == null || !_view.IsValid() || !_view.IsOwner()) return;

        switch (_state)
        {
            case WorkState.Idle: TickIdle(); break;
            case WorkState.ToSource: TickToSource(); break;
            case WorkState.ToDestination: TickToDestination(); break;
            case WorkState.ReturningHome: TickReturningHome(); break;
        }
    }

    private void TickIdle()
    {
        if (Time.time < _nextScan) return;
        _nextScan = Time.time + ScanInterval;

        if (TryFindBatch())
        {
            _state = WorkState.ToSource;
            return;
        }

        if (Vector3.Distance(transform.position, _home) > InteractionDistance)
            _state = WorkState.ReturningHome;
    }

    private void TickToSource()
    {
        if (!IsUsable(_source))
        {
            AbortBatch();
            return;
        }

        if (!MoveTowards(_source.transform.position)) return;

        var inventory = _source.GetInventory();
        if (inventory == null)
        {
            AbortBatch();
            return;
        }

        // The batch is planned remotely, but every stack is revalidated only
        // after the porter has physically reached the source chest.
        for (var i = _cargo.Count - 1; i >= 0; --i)
        {
            var entry = _cargo[i];
            if (entry.Item == null ||
                !inventory.ContainsItem(entry.Item) ||
                !IsUsable(entry.Destination) ||
                !QuickStackPlusBridge.Accepts(entry.Destination, entry.Item) ||
                !entry.Destination.GetInventory().CanAddItem(entry.Item, -1))
            {
                _cargo.RemoveAt(i);
            }
        }

        if (_cargo.Count == 0)
        {
            FinishBatch();
            return;
        }

        _state = WorkState.ToDestination;
    }

    private void TickToDestination()
    {
        if (!IsUsable(_source) || _cargo.Count == 0)
        {
            FinishBatch();
            return;
        }

        var index = FindNearestCargoIndex();
        if (index < 0)
        {
            FinishBatch();
            return;
        }

        var entry = _cargo[index];
        if (!IsUsable(entry.Destination) || entry.Item == null)
        {
            _cargo.RemoveAt(index);
            return;
        }

        if (!MoveTowards(entry.Destination.transform.position)) return;

        if (TransferService.TryMoveWholeStack(_source, entry.Destination, entry.Item))
            Plugin.Log.LogDebug($"Porter moved {entry.Item.m_shared.m_name}.");

        // Whether the transfer succeeded or failed, do not get stuck on this
        // destination. A failed stack will be reconsidered during the next scan.
        _cargo.RemoveAt(index);

        if (_cargo.Count == 0)
            FinishBatch();
    }

    private void TickReturningHome()
    {
        if (MoveTowards(_home))
        {
            _state = WorkState.Idle;
            _nextScan = 0f;
        }
    }

    private bool TryFindBatch()
    {
        ClearBatch();

        var radius = Plugin.WorkRadius.Value;
        var maxStacks = Mathf.Max(1, Plugin.MaxStacksPerTrip.Value);

        foreach (var candidateSource in Object.FindObjectsOfType<Container>())
        {
            if (!IsUsable(candidateSource) || !SourceContainerMarker.IsSource(candidateSource)) continue;
            if (Vector3.Distance(_home, candidateSource.transform.position) > radius) continue;

            var items = candidateSource.GetInventory()?.GetAllItems();
            if (items == null || items.Count == 0) continue;

            foreach (var candidateItem in items)
            {
                var target = FindDestination(candidateItem, candidateSource, radius);
                if (target == null) continue;

                _cargo.Add(new CargoEntry
                {
                    Item = candidateItem,
                    Destination = target
                });

                if (_cargo.Count >= maxStacks) break;
            }

            if (_cargo.Count == 0) continue;

            _source = candidateSource;
            return true;
        }

        return false;
    }

    private Container FindDestination(ItemDrop.ItemData item, Container source, float radius)
    {
        Container best = null;
        var bestDistance = float.MaxValue;

        foreach (var container in Object.FindObjectsOfType<Container>())
        {
            if (!IsUsable(container) || container == source || SourceContainerMarker.IsSource(container)) continue;
            if (Vector3.Distance(_home, container.transform.position) > radius) continue;
            if (!QuickStackPlusBridge.Accepts(container, item)) continue;

            var inventory = container.GetInventory();
            if (inventory == null || !inventory.CanAddItem(item, -1)) continue;

            var distance = Vector3.Distance(source.transform.position, container.transform.position);
            if (distance < bestDistance)
            {
                best = container;
                bestDistance = distance;
            }
        }

        return best;
    }

    private int FindNearestCargoIndex()
    {
        var bestIndex = -1;
        var bestDistance = float.MaxValue;

        for (var i = 0; i < _cargo.Count; ++i)
        {
            var destination = _cargo[i].Destination;
            if (!IsUsable(destination)) continue;

            var distance = Vector3.Distance(transform.position, destination.transform.position);
            if (distance < bestDistance)
            {
                bestIndex = i;
                bestDistance = distance;
            }
        }

        return bestIndex;
    }

    private bool MoveTowards(Vector3 point)
    {
        if (Vector3.Distance(transform.position, point) <= InteractionDistance) return true;

        var speed = _character != null ? 2.5f : 2f;
        transform.position = Vector3.MoveTowards(transform.position, point, speed * Time.deltaTime);

        var direction = point - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 8f * Time.deltaTime);

        return false;
    }

    private static bool IsUsable(Container c)
    {
        if (!c) return false;
        var view = c.GetComponent<ZNetView>();
        return view != null && view.IsValid();
    }

    private void FinishBatch()
    {
        ClearBatch();
        _state = WorkState.Idle;
        _nextScan = 0f;
    }

    private void AbortBatch()
    {
        ClearBatch();
        _state = WorkState.ReturningHome;
    }

    private void ClearBatch()
    {
        _source = null;
        _cargo.Clear();
    }
}
