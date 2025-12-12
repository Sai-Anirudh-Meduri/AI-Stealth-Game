using UnityEngine;

//Enter this state after being knifed in the back. Mostly handles animation logic before dying.
public class KnifedState : BaseState
{
    private float _deathTimer = 0f; //Tracks how long the player has befor dying.
    private float _deathDuration = 10f; //The cutoff time for when the player is declared dead.
    public KnifedState(EnemyStateMachineController controller, EnemyStateFactory factory) : base(controller, factory)
    {
    }
    public override void CheckSwitchState()
    {
        return;
    }

    //Do this when entering the state (ie was stabbed). TODO: More complex behavior, just changes material as a test.
    public override void EnterState()
    {
        //_controller.Renderer.material = _controller.SkinMaterial[1];
    }

    public override void ExitState()
    {
        return;
    }

    //Executes every frame. Tracks the death timer, triggers switch to dead state when timer elapses.
    public override void UpdateState()
    {
        _deathTimer += Time.deltaTime;

        if( _deathTimer >= _deathDuration )
        {
            this.SwitchState(_factory.DeadState());
        }
    }
}
