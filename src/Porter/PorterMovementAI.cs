using UnityEngine;

namespace FullingPorter.Porter;

/// <summary>
/// Passive vanilla movement driver. It keeps BaseAI's locomotion/pathfinding
/// without inheriting MonsterAI's combat, target selection or wandering.
/// </summary>
internal sealed class PorterMovementAI : BaseAI
{
    private void Awake()
    {
        m_pathAgentType = Pathfinding.AgentType.Humanoid;
    }

    internal void WalkTo(Vector3 point, float stopDistance)
    {
        MoveTo(Time.deltaTime, point, stopDistance, false);
    }

    internal void Halt()
    {
        StopMoving();
    }
}
