using UnityEngine;

public class CrouchingState : State
{
    float playerSpeed;
    bool belowCeiling;
    bool grounded;
    float gravityValue;
    Vector3 currentVelocity;

    // store original collider
    float originalHeight;
    Vector3 originalCenter;

    const float crouchHeight = 1.0f;      
    static readonly Vector3 crouchCenter = new Vector3(0f, 0.65f, 0f); 

    public CrouchingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        character.animator.SetTrigger("crouch");
        gravityVelocity.y = 0;
        playerSpeed = character.crouchSpeed;

        // Save original collider
        originalHeight = character.controller.height;
        originalCenter = character.controller.center;

        // Apply crouch collider settings
        character.controller.height = crouchHeight;
        character.controller.center = crouchCenter;

        grounded = character.controller.isGrounded;
        gravityValue = character.gravityValue;
    }

    public override void Exit()
    {
        base.Exit();
        character.animator.SetTrigger("move");

        // Restore normal collider
        character.controller.height = originalHeight;
        character.controller.center = originalCenter;

        gravityVelocity.y = 0f;
    }

    public override void HandleInput()
    {
        base.HandleInput();

        input = moveAction.ReadValue<Vector2>();
        velocity = new Vector3(input.x, 0, input.y);
        velocity = velocity.x * character.cameraTransform.right.normalized + velocity.z * character.cameraTransform.forward.normalized;
        velocity.y = 0f;

        if (crouchAction.triggered && !belowCeiling)
        {
            stateMachine.ChangeState(character.standing);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        character.animator.SetFloat("speed", input.magnitude, character.speedDampTime, Time.deltaTime);

        if (!character.healthSystem.IsStaminaFull())
        {
            character.healthSystem.RegenerateStamina(character.staminaRegenRate * Time.deltaTime);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        // Check for ceiling before standing
        belowCeiling = Physics.Raycast(
            character.transform.position + Vector3.up * (crouchHeight / 2f),
            Vector3.up,
            character.normalColliderHeight - crouchHeight,
            ~LayerMask.GetMask("Player")
        );

        gravityVelocity.y += gravityValue * Time.deltaTime;
        grounded = character.controller.isGrounded;

        if (grounded && gravityVelocity.y < 0)
            gravityVelocity.y = 0f;

        currentVelocity = Vector3.Lerp(currentVelocity, velocity, character.velocityDampTime);
        character.controller.Move(currentVelocity * Time.deltaTime * playerSpeed + gravityVelocity * Time.deltaTime);

        if (velocity.magnitude > 0)
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, Quaternion.LookRotation(velocity), character.rotationDampTime);
    }
}