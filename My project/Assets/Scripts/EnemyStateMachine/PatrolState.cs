using UnityEngine;
using static NoiseHandler;

public class PatrolState : BaseState, ICanHear, ICanBeDamaged
{
    private int currentIndex = 0;
    private float waitTimer = 0f;
    public PatrolState(EnemyStateMachineController controller, EnemyStateFactory factory) : base(controller, factory)
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

    public override void EnterState()
    {
        if (_controller.patrolPoints.Count == 0)
        {
            this.SwitchState(_factory.Idle());
        }

        _controller.MyState = EnemyStateType.Patrol;

        //First go to nearet patrol point
        float dist = float.MinValue;
        int startIndex = 0;
        foreach (Vector3 point in _controller.patrolPoints)
        {
            float newDist = Vector3.Distance(_controller.Trans.position, point);

            if (newDist > dist)
            {
                dist = newDist;
                currentIndex = startIndex;
            }
            startIndex++;
        }

        waitTimer = 0f;

        _controller.Agent.destination = _controller.patrolPoints[currentIndex];
        //_controller.Renderer.material = _controller.SkinMaterial[0];
    }

    public override void ExitState()
    {

    }

    public void getBackStabbed()
    {
        this.SwitchState(_factory.DieState());
    }

    public void HearNoise(NoiseID id, Transform origin, double range)
    {
        if (Vector3.Distance(origin.position, _controller.Trans.position) <= range)
        {
            _controller.Goal = origin.position;
            this.SwitchState(_factory.MoveToNoise());
        }
    }

    public override void UpdateState()
    {
        if (_controller.patrolPoints == null || _controller.patrolPoints.Count == 0)
            return;

        if (!_controller.Agent.pathPending && 
            _controller.Agent.remainingDistance <= _controller.Agent.stoppingDistance)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= _controller.patrolWait)
            {
                currentIndex = (currentIndex + 1) % _controller.patrolPoints.Count;
                _controller.Agent.destination = _controller.patrolPoints[currentIndex];
                waitTimer = 0f;
            }
        }

        CheckSwitchState();

        _controller.Anim.SetFloat(_controller.AnimHash[EnemyStateMachineController.AnimID.Speed],
                                  _controller.Agent.velocity.magnitude / _controller.Agent.speed);
    }

}
