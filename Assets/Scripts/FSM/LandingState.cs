using UnityEngine;
 
public class LandingState:State
{
    float timePassed;
    float landingTime;
 
    float gravityValue;
    Vector3 currentVelocity;
    Vector3 cVelocity;

    public LandingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }
 
    public override void Enter()
    {
        base.Enter();
        timePassed = 0f;
        character.animator.SetTrigger("land");
        landingTime = 0.3f; 

        currentVelocity = new Vector3(character.playerVelocity.x, 0, character.playerVelocity.z);
        gravityValue = character.gravityValue;
    }

    public override void HandleInput()
    {
        base.HandleInput();
        
        input = moveAction.ReadValue<Vector2>();

    }
 
    public override void LogicUpdate()
    {
        base.LogicUpdate();
        
        character.animator.SetFloat("speed", input.magnitude, character.speedDampTime, Time.deltaTime);
        
        if (!sprintAction.IsPressed() && !character.healthSystem.IsStaminaFull())
        {
            character.healthSystem.RegenerateStamina(character.staminaRegenRate * Time.deltaTime);
        }

        if (timePassed > landingTime)
        {
            character.animator.SetTrigger("move");

            if (sprintAction.IsPressed() && input.magnitude > 0)
            {
                stateMachine.ChangeState(character.sprinting);
            }
            else
            {
                stateMachine.ChangeState(character.standing);
            }
        }
        timePassed += Time.deltaTime;
    }

    public override void PhysicsUpdate(float speedModifier)
    {
        base.physicsUpdate(speedModifier);

        gravityVelocity.y += gravityValue * Time.deltaTime;
        if (character.controller.isGrounded && gravityVelocity.y < 0)
        {
            gravityVelocity.y = 0f;
        }

        velocity = new Vector3(input.x, 0, input.y);
        velocity = velocity.x * character.cameraTransform.right.normalized + velocity.z * character.cameraTransform.forward.normalized;
        velocity.y = 0f;

        velocity *= character.playerSpeed;

        currentVelocity = Vector3.SmoothDamp(currentVelocity, velocity, ref cVelocity, character.velocityDampTime);
        character.controller.Move(currentVelocity * speedModifier * Time.deltaTime + gravityVelocity * Time.deltaTime);
        
        if (velocity.sqrMagnitude > 0)
        {
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, Quaternion.LookRotation(velocity), character.rotationDampTime);
        }
    }

    public override void Exit()
    {
        base.Exit();
        character.playerVelocity = currentVelocity;
    }
}