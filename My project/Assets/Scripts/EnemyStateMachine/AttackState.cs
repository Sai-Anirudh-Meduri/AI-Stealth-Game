using UnityEngine;

//State for shoot at the player. Enemy stops moving, aims, and shoots. Accuracy increases over time.
public class AttackState : BaseState, ICanBeDamaged
{
    protected PursuitState _pursuit;
    private float _attackTimer;
    public AttackState(EnemyStateMachineController controller, EnemyStateFactory factory) : base(controller, factory)
    {
        _pursuit = (PursuitState)factory.PursuitState();
    }

    public override void CheckSwitchState()
    {
        //If the player is too far away and we are not in the middle of shooting, go back to pursuit
        if(!_controller.Shooting &&
            Vector3.Distance(_controller.Trans.position, _controller.Player.position) > _controller.AttackRange)
        {
            //Switch to pursuit. Let the pursuit state decide when to go back to default
            this.SwitchState(_pursuit);
        }
    }

    public override void EnterState()
    {
        _controller.Accuracy = _controller.BaseAccuracy; //Reset to base accuracy every time we enter state.
        _attackTimer = _controller.AttackTimer;
        _controller.Agent.isStopped = true;
        _controller.Agent.updateRotation = true; //Stop moving
        //_controller.Renderer.material = _controller.SkinMaterial[4];
    }

    public override void ExitState()
    {
        _controller.Agent.isStopped = false;
        _controller.Agent.updateRotation = false; //Allow the agent to move again when they leave attacking state
    }

    public override void UpdateState()
    {
        CheckSwitchState();
        _controller.Anim.SetFloat(_controller.AnimHash[EnemyStateMachineController.AnimID.Speed], 0f);

        Vector3 direction = _controller.Player.position - _controller.Trans.position;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            _controller.Trans.rotation = Quaternion.RotateTowards(
                _controller.Trans.rotation,
                targetRot,
                100 * Time.deltaTime
            );
        }

        if (_attackTimer <= 0)
        {
            _attackTimer = _controller.AttackTimer;
            _controller.Anim.SetTrigger(_controller.AnimHash[EnemyStateMachineController.AnimID.Attack]);
        }
        else
        {
            _attackTimer -= Time.deltaTime;
        }
    }

    //React to being stabbed in the back.
    public void getBackStabbed()
    {
        this.SwitchState(_factory.DieState());
    }
}
