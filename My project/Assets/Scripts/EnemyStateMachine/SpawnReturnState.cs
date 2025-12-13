using UnityEngine;
using UnityEngine.AI;

public class SpawnReturnState : BaseState
{
    private Vector3 _returnPos;
    private EnemySpawnerController _spawner;
    public SpawnReturnState(EnemyStateMachineController controller, EnemyStateFactory factory, Vector3 pos, EnemySpawnerController spawner)
        : base(controller, factory)
    {
        _returnPos = pos;
        _spawner = spawner;
    }

    public override void CheckSwitchState()
    {
        //Check if the player is seen
        if (_controller.playerVision())
        {
            //Player was seen, switch to pursuit
            AlertManager.instance.CallSpawner(_controller.Trans.position); //Gets backup by triggering the nearest spawner to spawn enemies already in pursuit.
            this.SwitchState(_factory.SpawnPursuitState(_returnPos, _spawner));
        }
    }

    public override void EnterState()
    {
        _controller.Goal = _returnPos;
        _controller.Agent.destination = _controller.Goal; //Go back to the return position
        //For debug, go to new material
        //_controller.Renderer.material = _controller.SkinMaterial[2];
    }

    public override void ExitState()
    {
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        _controller.Anim.SetFloat(_controller.AnimHash[EnemyStateMachineController.AnimID.Speed],
                                  _controller.Agent.velocity.magnitude / _controller.Agent.speed);

        //When we reach the destination, destroy this enemy object
        if (!_controller.Agent.pathPending &&
            _controller.Agent.remainingDistance <= _controller.Agent.stoppingDistance &&
            (!_controller.Agent.hasPath || _controller.Agent.velocity.sqrMagnitude < 0.01f))
        {
            _spawner.activated--;
            _controller.destroySelf();
        }
    }
}
