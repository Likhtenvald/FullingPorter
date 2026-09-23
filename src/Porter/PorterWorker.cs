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
    private readonly Dictionary<Container, Dictionary<string, float>> _destinationCooldowns = new();
    private Vector3 _home;
    private float _nextScan;
    private Vector3 _moveTarget;
    private Vector3 _lastProgressPosition;
    private float _lastProgressTime;
    private bool _trackingMove;
    private float _nextTransferTime;
    private bool _homeInitialized;
    private int _activeTripCapacity;

    private const float InteractionDistance = 2.5f;
    private const float HomeDistance = 0.35f;
    private const float ScanInterval = 2f;
    private const float StuckTimeout = 8f;
    private const float ProgressDistance = 0.25f;
    private const float TransferInterval = 0.12f;
    private const float DestinationRetryCooldown = 8f;

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
        if (_view == null || !_view.IsValid() || ZNet.instance == null) return;

        // All porter work is server-authoritative. Clients render the replicated
        // result but never mutate container inventories themselves.
        if (!ZNet.instance.IsServer())
            return;

        if (!_view.IsOwner())
            _view.ClaimOwnership();
        if (!_view.IsOwner())
            return;

        if (!_homeInitialized)
        {
            var state = GetComponent<PorterState>();
            _home = state != null ? state.GetOrCreateHome(transform.position) : transform.position;
            _homeInitialized = true;
        }

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

        if ((transform.position - _home).sqrMagnitude > HomeDistance * HomeDistance)
            _state = WorkState.ReturningHome;
    }

    private void TickToSource()
    {
        if (!IsUsable(_source))
        {
            AbortBatch();
            return;
        }

        if (!MoveTowards(_source.transform.position, InteractionDistance, out var stuckAtSource))
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

        if (!MoveTowards(entry.Destination.transform.position, InteractionDistance, out var stuckAtDestination))
        {
            if (stuckAtDestination)
            {
                Plugin.Log.LogDebug("Porter could not reach a destination; skipping this stack.");
                _cargo.RemoveAt(index);
                if (_cargo.Count == 0) FinishBatch();
            }
            return;
        }

        // Inventory and ZDO changes are intentionally rate-limited. Without this,
        // a 12-stack batch can generate a burst of container/network updates over
        // only a handful of rendered frames.
        if (Time.time < _nextTransferTime)
            return;

        _nextTransferTime = Time.time + TransferInterval;

        if (TransferService.TryMoveWholeStack(_source, entry.Destination, entry.Item))
        {
            ClearDestinationCooldown(entry.Destination, entry.Item);
            Plugin.Log.LogDebug($"Porter moved {entry.Item.m_shared.m_name}.");
        }
        else
        {
            SetDestinationCooldown(entry.Destination, entry.Item);
        }

        // Whether the transfer succeeded or failed, do not get stuck on this
        // destination. A failed stack will be reconsidered during the next scan.
        _cargo.RemoveAt(index);

        if (_cargo.Count == 0)
            FinishBatch();
    }

    private void TickReturningHome()
    {
        if (MoveTowards(_home, HomeDistance, out var stuckReturningHome) || stuckReturningHome)
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
        var radiusSqr = radius * radius;
        var maxStacks = Mathf.Max(1, Plugin.MaxStacksPerTrip.Value);
        _activeTripCapacity = maxStacks;

        // Unity scene-wide searches are relatively expensive. Take one snapshot
        // for the whole planning pass instead of repeating FindObjectsOfType for
        // every candidate item.
        var containers = Object.FindObjectsOfType<Container>();
        var acceptedIds = new Dictionary<Container, HashSet<string>>();

        foreach (var candidateSource in containers)
        {
            if (!IsUsable(candidateSource) || !SourceContainerMarker.IsSource(candidateSource)) continue;
            if ((candidateSource.transform.position - _home).sqrMagnitude > radiusSqr) continue;

            var items = candidateSource.GetInventory()?.GetAllItems();
            if (items == null || items.Count == 0) continue;

            foreach (var candidateItem in items)
            {
                var target = FindDestination(candidateItem, candidateSource, radiusSqr, containers, acceptedIds);
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

    private Container FindDestination(
        ItemDrop.ItemData item,
        Container source,
        float radiusSqr,
        Container[] containers,
        Dictionary<Container, HashSet<string>> acceptedIds)
    {
        if (item?.m_dropPrefab == null) return null;

        Container best = null;
        var bestDistance = float.MaxValue;
        var itemId = item.m_dropPrefab.name;

        foreach (var container in containers)
        {
            if (!IsUsable(container) || container == source || SourceContainerMarker.IsSource(container)) continue;
            if ((container.transform.position - _home).sqrMagnitude > radiusSqr) continue;

            if (!acceptedIds.TryGetValue(container, out var accepted))
            {
                accepted = QuickStackPlusBridge.GetAcceptedItemIds(container);
                acceptedIds[container] = accepted;
            }

            if (!accepted.Contains(itemId)) continue;
            if (IsDestinationCoolingDown(container, itemId)) continue;

            var inventory = container.GetInventory();
            if (inventory == null) continue;
            if (!inventory.CanAddItem(item, -1))
            {
                SetDestinationCooldown(container, item);
                continue;
            }

            var distanceSqr = (source.transform.position - container.transform.position).sqrMagnitude;
            if (distanceSqr < bestDistance)
            {
                best = container;
                bestDistance = distanceSqr;
            }
        }

        return best;
    }

    internal string GetStatusText()
    {
        var capacity = _activeTripCapacity > 0
            ? _activeTripCapacity
            : Mathf.Max(1, Plugin.MaxStacksPerTrip.Value);

        switch (_state)
        {
            case WorkState.ToSource:
                return $"$fullingporter_status_collecting ({_cargo.Count}/{capacity})";
            case WorkState.ToDestination:
                return $"$fullingporter_status_delivering ({_cargo.Count}/{capacity})";
            case WorkState.ReturningHome:
                return "$fullingporter_status_returning";
            default:
                return "$fullingporter_status_idle";
        }
    }

    private bool IsDestinationCoolingDown(Container container, string itemId)
    {
        if (container == null || string.IsNullOrEmpty(itemId)) return false;
        if (!_destinationCooldowns.TryGetValue(container, out var items)) return false;
        if (!items.TryGetValue(itemId, out var until)) return false;

        if (Time.time < until)
            return true;

        items.Remove(itemId);
        if (items.Count == 0)
            _destinationCooldowns.Remove(container);
        return false;
    }

    private void SetDestinationCooldown(Container container, ItemDrop.ItemData item)
    {
        var itemId = item?.m_dropPrefab?.name;
        if (container == null || string.IsNullOrEmpty(itemId)) return;

        if (!_destinationCooldowns.TryGetValue(container, out var items))
        {
            items = new Dictionary<string, float>();
            _destinationCooldowns[container] = items;
        }

        items[itemId] = Time.time + DestinationRetryCooldown;
    }

    private void ClearDestinationCooldown(Container container, ItemDrop.ItemData item)
    {
        var itemId = item?.m_dropPrefab?.name;
        if (container == null || string.IsNullOrEmpty(itemId)) return;
        if (!_destinationCooldowns.TryGetValue(container, out var items)) return;

        items.Remove(itemId);
        if (items.Count == 0)
            _destinationCooldowns.Remove(container);
    }

    private int FindNearestCargoIndex()
    {
        var bestIndex = -1;
        var bestDistance = float.MaxValue;

        for (var i = 0; i < _cargo.Count; ++i)
        {
            var destination = _cargo[i].Destination;
            if (!IsUsable(destination)) continue;

            var distanceSqr = (transform.position - destination.transform.position).sqrMagnitude;
            if (distanceSqr < bestDistance)
            {
                bestIndex = i;
                bestDistance = distanceSqr;
            }
        }

        return bestIndex;
    }

    private bool MoveTowards(Vector3 point, float stopDistance, out bool stuck)
    {
        stuck = false;

        if (DistanceXZ(transform.position, point) <= stopDistance)
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

        _ai.WalkTo(point, stopDistance);
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

        if ((transform.position - _home).sqrMagnitude > HomeDistance * HomeDistance)
        {
            _state = WorkState.ReturningHome;
            return;
        }

        _state = WorkState.Idle;
        _nextScan = Time.time + ScanInterval;
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
        _nextTransferTime = 0f;
        _activeTripCapacity = 0;
    }
}
