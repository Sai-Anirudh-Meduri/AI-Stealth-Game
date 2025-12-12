using UnityEngine;
using UnityEngine.AI;

//State for chasing the player
public class SpawnPursuitState : PursuitState
{
    Vector3 _returnPos;
    private EnemySpawnerController _spawner;

    public SpawnPursuitState(EnemyStateMachineController controller, EnemyStateFactory factory, Vector3 pos, EnemySpawnerController spawner) :
        base(controller, factory)
    {
        _returnPos = pos;
        _spawner = spawner;
    }

    public override void CheckSwitchState()
    {
        //If we are close enough, go on the offensive
        if(_controller.playerVision() &&
           Vector3.Distance(_controller.Trans.position, _controller.Player.position) < _controller.AttackRange)
        {
            this.SwitchState(_factory.SpawnAttackState(_returnPos, _spawner));
        }
        //If we reach the destination without switching to an attack state, the player is not there anymore
        else if (_controller.HasReachedDestination() && !_controller.playerVision())
        {
            this.SwitchState(_factory.SpawnReturnState(_returnPos, _spawner)); //Switch back to the default state, end pursuit
        }
    }

    public override void EnterState()
    {
        base.EnterState();

        _controller.MyState = EnemyStateType.SpawnPursuit;
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
