using UnityEngine;

//This class controls how the enemy behaves while moving towards a noise (player is not seen yet).
public class MoveToNoiseState : BaseState, ICanBeDamaged
{
    public MoveToNoiseState(EnemyStateMachineController controller, EnemyStateFactory factory) : base(controller, factory)
    {
    }

    public override void CheckSwitchState()
    {
        //Check if the player is seen
        if (_controller.playerVision())
        {
            //Player was seen, switch to pursuit
            AlertManager.instance.CallSpawner(_controller.Trans.position); //Gets backup by triggering the nearest spawner to spawn enemies already in pursuit.
            this.SwitchState(_factory.PursuitState());
        }
    }

    //Enter the state and set the navagent to move to the noise
    public override void EnterState()
    {
        _controller.Agent.destination = _controller.Goal;
        _controller.MyState = EnemyStateType.MoveToNoise;
        Debug.Log("Moving to: x:" + _controller.Agent.destination.x + 
                  " y: "+ _controller.Agent.destination.y + 
                  " z: " + _controller.Agent.destination.z);
    }

    public override void ExitState()
    {

    }

    //Called when the player knife hits the enemy from behind.
    public void getBackStabbed()
    {
        this.SwitchState(_factory.DieState());
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        _controller.Anim.SetFloat(_controller.AnimHash[EnemyStateMachineController.AnimID.Speed],
                                  _controller.Agent.velocity.magnitude / _controller.Agent.speed);
    }
}
