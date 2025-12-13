//Used to generate new instances of states when the enemy switches their state.
using UnityEngine;

public class EnemyStateFactory
{
    private EnemyStateMachineController _controller;
    
    public EnemyStateFactory(EnemyStateMachineController controller)
    {
        _controller = controller;
    }

    public BaseState Idle()
    {
        return new IdleState(_controller, this);
    }

    public BaseState Patrol()
    {
        return new PatrolState(_controller, this);
    }

    public BaseState MoveToNoise()
    {
        return new MoveToNoiseState(_controller, this);
    }
    public BaseState DieState()
    {
        return new DieState(_controller, this);
    }
    public BaseState DeadState()
    {
        return new DeadState(_controller, this);
    }

    public BaseState PursuitState()
    {
        return new PursuitState(_controller, this);
    }

    public BaseState AttackState()
    {
        return new AttackState(_controller, this);
    }

    public BaseState SpawnPursuitState(Vector3 pos, EnemySpawnerController spawner)
    {
        return new SpawnPursuitState (_controller, this, pos, spawner);
    }

    public BaseState SpawnAttackState(Vector3 pos, EnemySpawnerController spawner)
    {
        return new SpawnAttackState(_controller, this, pos, spawner);
    }
    public BaseState SpawnReturnState(Vector3 pos, EnemySpawnerController spawner)
    {
        return new SpawnReturnState(_controller, this, pos, spawner);
    }
}
