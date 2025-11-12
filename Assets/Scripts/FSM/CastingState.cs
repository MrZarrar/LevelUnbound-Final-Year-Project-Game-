using UnityEngine;

public class CastingState : State
{
    float timePassed;
    float clipLength;
    float clipSpeed;
    float clipDuration;

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
        
        
        character.animator.SetTrigger("cast"); 
        character.animator.SetFloat("speed", 0f);

        
        clipDuration = float.MaxValue;
    }

    public override void HandleInput()
    {
        base.HandleInput();
        
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        timePassed += Time.deltaTime;

        AnimatorClipInfo[] clipInfo = character.animator.GetCurrentAnimatorClipInfo(1);

        if (clipInfo.Length > 0)
        {
            AnimatorStateInfo stateInfo = character.animator.GetCurrentAnimatorStateInfo(1);
            clipLength = clipInfo[0].clip.length;
            clipSpeed = stateInfo.speed; 
            clipDuration = clipLength / clipSpeed;
        }

        
        if (timePassed >= clipDuration)
        {
            stateMachine.ChangeState(character.combatting);
            character.animator.SetTrigger("move");
        }
    }

    public override void Exit()
    {
        base.Exit();
        character.animator.applyRootMotion = false;
    }
}