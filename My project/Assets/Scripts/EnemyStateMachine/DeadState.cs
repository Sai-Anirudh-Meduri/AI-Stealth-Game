using UnityEngine;

//Enter this state after being killed. Mostly cleans up before despawning.
public class DeadState : BaseState
{
    public DeadState(EnemyStateMachineController controller, EnemyStateFactory factory) : base(controller, factory)
    {
    }

    public override void CheckSwitchState()
    {
        return;
    }

    //When entering the state, do this. TODO: More complex behavior, currently just changes material as a test
    public override void EnterState()
    {
        //_controller.Renderer.material = _controller.SkinMaterial[2];
    }

    public override void ExitState()
    {
        return;
    }

    public override void UpdateState()
    {
        return;
    }
}