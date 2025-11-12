using UnityEngine;

public class CastingState : State
{
    float timePassed;
    private float castDuration = 1.0f;

    public CastingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        character.animator.applyRootMotion = true;
        timePassed = 0f;
    
    }

    public override void HandleInput()
    {
        base.HandleInput();
        
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        timePassed += Time.deltaTime;

        if (timePassed >= castDuration)
        {
            stateMachine.ChangeState(character.previousMovementState);            

            character.animator.SetTrigger("move");
        }
    }

    public override void Exit()
    {
        base.Exit();
        character.animator.applyRootMotion = false;
    }
}