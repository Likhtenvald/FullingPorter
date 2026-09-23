using System.Collections.Generic;
using FullingPorter.Storage;
using UnityEngine;

namespace FullingPorter.Porter;

internal sealed class PorterWorker : MonoBehaviour
{
    private enum WorkState { Idle, ToSource, ToDestination, ReturningHome }

    private Character _character;
    private MonsterAI _ai;
    private ZNetView _view;
    private WorkState _state;
    private Container _source;
    private Container _destination;
    private ItemDrop.ItemData _cargo;
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

        if (!TryFindJob(out _source, out _destination, out _cargo)) return;
        _state = WorkState.ToSource;
    }

    private void TickToSource()
    {
        if (!IsUsable(_source) || _cargo == null) { ResetJob(); return; }
        if (!MoveTowards(_source.transform.position)) return;

        // The item reference is revalidated before mutation.
        if (!_source.GetInventory().ContainsItem(_cargo)) { ResetJob(); return; }
        _state = WorkState.ToDestination;
    }

    private void TickToDestination()
    {
        if (!IsUsable(_source) || !IsUsable(_destination) || _cargo == null) { ResetJob(); return; }
        if (!MoveTowards(_destination.transform.position)) return;

        if (TransferService.TryMoveWholeStack(_source, _destination, _cargo))
            Plugin.Log.LogDebug($"Porter moved {_cargo.m_shared.m_name}.");

        ResetJob();
    }

    private void TickReturningHome()
    {
        if (MoveTowards(_home)) _state = WorkState.Idle;
    }

    private bool TryFindJob(out Container source, out Container destination, out ItemDrop.ItemData item)
    {
        source = null; destination = null; item = null;
        var radius = Plugin.WorkRadius.Value;

        foreach (var candidateSource in Object.FindObjectsOfType<Container>())
        {
            if (!IsUsable(candidateSource) || !SourceContainerMarker.IsSource(candidateSource)) continue;
            if (Vector3.Distance(_home, candidateSource.transform.position) > radius) continue;

            var items = candidateSource.GetInventory()?.GetAllItems();
            if (items == null) continue;

            foreach (var candidateItem in items)
            {
                var target = FindDestination(candidateItem, candidateSource, radius);
                if (target == null) continue;
                source = candidateSource; destination = target; item = candidateItem;
                return true;
            }
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
            if (!container.GetInventory().CanAddItem(item, -1)) continue;

            var distance = Vector3.Distance(source.transform.position, container.transform.position);
            if (distance < bestDistance) { best = container; bestDistance = distance; }
        }
        return best;
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

    private void ResetJob()
    {
        _source = null; _destination = null; _cargo = null;
        _state = WorkState.ReturningHome;
    }
}
