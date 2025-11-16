using UnityEngine;

public class AttackState : State
{
    float timePassed;
    float clipLength;
    float clipSpeed;
    float clipDuration;
    bool attack;

    float gravityValue;
    Vector3 currentVelocity;
    bool grounded;
    float playerSpeed;
    Vector3 cVelocity;

    public AttackState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        attack = false;
        character.animator.applyRootMotion = false;
        
        timePassed = 0f;
        character.animator.SetTrigger("attack");

        input = Vector2.zero;
        currentVelocity = Vector3.zero;
        gravityVelocity.y = 0;
        
        playerSpeed = character.playerSpeed * 0.5f; 
        
        grounded = character.controller.isGrounded;
        gravityValue = character.gravityValue;
        
        clipDuration = float.MaxValue; 
    }

    public override void HandleInput()
    {
        base.HandleInput();

        if (attackAction.triggered)
        {
            attack = true;
        }

        input = moveAction.ReadValue<Vector2>();
        velocity = new Vector3(input.x, 0, input.y);

        velocity = velocity.x * character.cameraTransform.right.normalized + velocity.z * character.cameraTransform.forward.normalized;
        velocity.y = 0f;
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();

        character.animator.SetFloat("speed", input.magnitude, character.speedDampTime, Time.deltaTime);

        timePassed += Time.deltaTime;

        AnimatorClipInfo[] clipInfo = character.animator.GetCurrentAnimatorClipInfo(1);
        AnimatorStateInfo stateInfo = character.animator.GetCurrentAnimatorStateInfo(1);

        if (clipInfo.Length > 0 && stateInfo.IsTag("Attack"))
        {
            clipLength = clipInfo[0].clip.length;
            clipSpeed = stateInfo.speed;
            clipDuration = clipLength / clipSpeed;
        }


        if (timePassed >= clipDuration && attack)
        {
            stateMachine.ChangeState(character.attacking);
        }

        if (timePassed >= clipDuration)
        {
            stateMachine.ChangeState(character.combatting);
            character.animator.SetTrigger("move");
        }
    }

    public override void PhysicsUpdate(float speedModifier)
    {
        base.PhysicsUpdate(speedModifier);

        gravityVelocity.y += gravityValue * Time.deltaTime;
        grounded = character.controller.isGrounded;

        if (grounded && gravityVelocity.y < 0)
        {
            gravityVelocity.y = 0f;
        }

        currentVelocity = Vector3.SmoothDamp(currentVelocity, velocity, ref cVelocity, character.velocityDampTime);
        
        character.controller.Move(currentVelocity * Time.deltaTime * playerSpeed * speedModifier + gravityVelocity * Time.deltaTime);

        if (velocity.sqrMagnitude > 0)
        {
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, Quaternion.LookRotation(velocity), character.rotationDampTime);
        }
    }

    public override void Exit()
    {
        base.Exit();
        character.animator.applyRootMotion = false;
    }
}