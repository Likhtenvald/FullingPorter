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
    private PorterMovementAI _ai;
    private ZNetView _view;
    private WorkState _state;
    private Container _source;
    private readonly List<CargoEntry> _cargo = new();
    private Vector3 _home;
    private float _nextScan;
    private Vector3 _moveTarget;
    private Vector3 _lastProgressPosition;
    private float _lastProgressTime;
    private bool _trackingMove;

    private const float InteractionDistance = 2.5f;
    private const float ScanInterval = 2f;
    private const float StuckTimeout = 8f;
    private const float ProgressDistance = 0.25f;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _ai = GetComponent<PorterMovementAI>();
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

        if (!MoveTowards(_source.transform.position, out var stuckAtSource))
        {
            if (stuckAtSource) AbortBatch();
            return;
        }

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

        if (!MoveTowards(entry.Destination.transform.position, out var stuckAtDestination))
        {
            if (stuckAtDestination)
            {
                Plugin.Log.LogDebug("Porter could not reach a destination; skipping this stack.");
                _cargo.RemoveAt(index);
                if (_cargo.Count == 0) FinishBatch();
            }
            return;
        }

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
        if (MoveTowards(_home, out var stuckReturningHome) || stuckReturningHome)
        {
            _ai?.Halt();
            ResetMoveTracking();
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

    private bool MoveTowards(Vector3 point, out bool stuck)
    {
        stuck = false;

        if (DistanceXZ(transform.position, point) <= InteractionDistance)
        {
            _ai?.Halt();
            ResetMoveTracking();
            return true;
        }

        if (_ai == null)
        {
            Plugin.Log.LogWarning("Porter movement AI is missing.");
            stuck = true;
            return false;
        }

        if (!_trackingMove || DistanceXZ(_moveTarget, point) > 0.5f)
        {
            _trackingMove = true;
            _moveTarget = point;
            _lastProgressPosition = transform.position;
            _lastProgressTime = Time.time;
        }
        else if (DistanceXZ(_lastProgressPosition, transform.position) >= ProgressDistance)
        {
            _lastProgressPosition = transform.position;
            _lastProgressTime = Time.time;
        }
        else if (Time.time - _lastProgressTime >= StuckTimeout)
        {
            _ai.Halt();
            ResetMoveTracking();
            stuck = true;
            return false;
        }

        _ai.WalkTo(point, InteractionDistance);
        return false;
    }

    private static float DistanceXZ(Vector3 a, Vector3 b)
    {
        var dx = a.x - b.x;
        var dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    private void ResetMoveTracking()
    {
        _trackingMove = false;
        _moveTarget = Vector3.zero;
        _lastProgressPosition = Vector3.zero;
        _lastProgressTime = 0f;
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
        _ai?.Halt();
        ResetMoveTracking();
        _source = null;
        _cargo.Clear();
    }
}
