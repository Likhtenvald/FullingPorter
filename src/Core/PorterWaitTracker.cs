using System;

namespace FullingPorter.Core;

internal enum PorterWaitDecision
{
    Started,
    Waiting,
    TimedOut,
    LockTimedOut
}

/// <summary>
/// Tracks wait deadlines across a delivery attempt. Player access, porter lock
/// contention and container ownership have separate clocks. Reset between
/// trips so a previous wait cannot abort newly planned cargo.
/// </summary>
internal sealed class PorterWaitTracker
{
    private float _playerSince = -1f;
    private float _lockSince = -1f;
    private float _ownershipSince;
    private object _ownershipItem;

    internal PorterWaitDecision PlayerBusy(float now, float timeout)
    {
        _lockSince = -1f;
        ClearOwnership();

        var started = _playerSince < 0f;
        if (started)
            _playerSince = now;

        if (now - _playerSince >= timeout)
            return PorterWaitDecision.TimedOut;

        return started ? PorterWaitDecision.Started : PorterWaitDecision.Waiting;
    }

    internal void PlayerAvailable() => _playerSince = -1f;

    internal PorterWaitDecision LockBusy(float now, float timeout)
    {
        _playerSince = -1f;
        ClearOwnership();

        var started = _lockSince < 0f;
        if (started)
            _lockSince = now;

        if (now - _lockSince >= timeout)
            return PorterWaitDecision.TimedOut;

        return started ? PorterWaitDecision.Started : PorterWaitDecision.Waiting;
    }

    internal PorterWaitDecision OwnershipPending(
        object item,
        float now,
        float ownershipTimeout,
        float lockTimeout)
    {
        _playerSince = -1f;

        // Alternating lock and ownership retries must not extend the lock
        // deadline indefinitely.
        if (_lockSince >= 0f && now - _lockSince >= lockTimeout)
            return PorterWaitDecision.LockTimedOut;

        if (!ReferenceEquals(_ownershipItem, item))
        {
            _ownershipItem = item;
            _ownershipSince = now;
            return PorterWaitDecision.Started;
        }

        return now - _ownershipSince >= ownershipTimeout
            ? PorterWaitDecision.TimedOut
            : PorterWaitDecision.Waiting;
    }

    internal void Reset()
    {
        _playerSince = -1f;
        _lockSince = -1f;
        ClearOwnership();
    }

    private void ClearOwnership()
    {
        _ownershipItem = null;
        _ownershipSince = 0f;
    }
}
