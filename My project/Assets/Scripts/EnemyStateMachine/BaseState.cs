using UnityEngine;
using static NoiseHandler;

//Defines the template for states
public abstract class BaseState
{
    protected EnemyStateMachineController _controller; //The instance of the enemy that controls what state the enemy is in and most of the variables
    protected EnemyStateFactory _factory; //The script designed to help the enemy switch to new states.

    //Assign varables in constructor to ensure they are linked to the correct enemy by a script.
    public BaseState(EnemyStateMachineController controller, EnemyStateFactory factory)
    {
        _controller = controller;
        _factory = factory;
    }

    //Abstract: Function to execute when entering a state. (May be used to set parameters in the controller to state specific values)
    public abstract void EnterState();

    //Abstract: Function to execute every frame while in the current state (analogous to Unity Update)
    public abstract void UpdateState();

    //Abstract: Function to call when exiting a state (Used to perform logic such as cleaning u controller variables which must be done on exit)
    public abstract void ExitState();

    //Can be used if the state has internal logic for when it might switch to another state
    public abstract void CheckSwitchState();

    //NOT ABSTRACT. This is the basic logic used to switch from any given state to another state.
    protected void SwitchState(BaseState newState)
    {
        ExitState();

        newState.EnterState();

        _controller.CurrentState = newState;
    }
    //Overload to allow switching based on Enum
    protected void SwitchState(EnemyStateType newStateNum)
    {
        ExitState();

        BaseState newState = newStateNum switch
        {
            EnemyStateType.Idle => _factory.Idle(),
            EnemyStateType.Patrol => _factory.Patrol(),
            EnemyStateType.MoveToNoise => _factory.MoveToNoise(),
            _ => _factory.Idle()
        };

        _controller.CurrentState = newState;

        newState.EnterState();
    }
}

//These Interfaces enable functionality that only some states use and other don't.
//This marks a state in which the enemy can take damage. Defines fucntions other scripts can use to damage the enemy
public interface ICanBeDamaged
{
    void getBackStabbed(); //Function for invoking a backstab on the enemy.
}

//This marks a state where the enemy is able to react to noises.
public interface ICanHear
{
    void HearNoise(NoiseID id, Transform origin, double range); //Fuction invoked to make enemy react to a noise.
}