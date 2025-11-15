using UnityEngine;

public class SprintJumpState : State
{
    float gravityValue;
    float playerSpeed;

    Vector3 currentVelocity;
    Vector3 cVelocity;

    public SprintJumpState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();
        
        character.animator.applyRootMotion = false; 
        character.animator.SetTrigger("sprintJump");
        
        gravityValue = character.gravityValue;

        character.playerVelocity.y = Mathf.Sqrt(character.jumpHeight * -2f * gravityValue);


        currentVelocity = new Vector3(character.playerVelocity.x, 0, character.playerVelocity.z);
        
        velocity = currentVelocity; 
        
    }

    public override void Exit()
    {
        base.Exit();
        character.playerVelocity = currentVelocity * playerSpeed;
    }

    public override void HandleInput()
    {
        base.HandleInput();

        input = moveAction.ReadValue<Vector2>();
        

        velocity = new Vector3(input.x, 0, input.y);
        velocity = velocity.x * character.cameraTransform.right.normalized + velocity.z * character.cameraTransform.forward.normalized;
        velocity.y = 0f;
        

        velocity *= character.sprintSpeed;
    }
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        if (character.controller.isGrounded)
        {
            // Transition to the landing state
            stateMachine.ChangeState(character.landing);
        }
    }
 
    public override void PhysicsUpdate(float speedModifier)
    {
        base.physicsUpdate(speedModifier);

        character.playerVelocity.y += gravityValue * Time.deltaTime;

        currentVelocity = Vector3.SmoothDamp(currentVelocity, velocity, ref cVelocity, character.velocityDampTime);

        Vector3 combinedVelocity = currentVelocity + new Vector3(0, character.playerVelocity.y, 0);
        

        
        character.controller.Move(combinedVelocity * Time.deltaTime);
    }
}