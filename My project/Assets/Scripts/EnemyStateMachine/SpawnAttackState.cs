using UnityEngine;

//State for shoot at the player. Enemy stops moving, aims, and shoots. Accuracy increases over time.
public class SpawnAttackState : AttackState
{
    private Vector3 _returnPos;
    private EnemySpawnerController _spawner;

    public SpawnAttackState(EnemyStateMachineController controller, EnemyStateFactory factory, Vector3 pos, EnemySpawnerController spawner)
        : base(controller, factory)
    {
        _returnPos = pos;
        _spawner = spawner;

        _pursuit = (SpawnPursuitState)factory.SpawnPursuitState(pos, spawner);
    }

    public override void CheckSwitchState()
    {
        base.CheckSwitchState();
    }

    public override void EnterState()
    {
        base.EnterState();
    }

    public override void ExitState()
    {
        base.ExitState();
    }

    public override void UpdateState()
    {
        base.UpdateState();
    }
}
