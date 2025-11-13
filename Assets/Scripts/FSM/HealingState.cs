using UnityEngine;

public class HealingState : State
{
    private GameObject activeVFX;

    float gravityValue;
    Vector3 currentVelocity;
    bool grounded;
    float playerSpeed;
    Vector3 cVelocity;


    private float healthPerSecond = 10f; // How much HP you heal
    private float manaCostPerSecond = 30f; // How much mana it costs

    public HealingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }

    public override void Enter()
    {
        base.Enter();

        if (character.healVFX != null)
        {
            activeVFX = Object.Instantiate(character.healVFX, character.transform);
        }

        input = Vector2.zero;
        currentVelocity = Vector3.zero;
        gravityVelocity.y = 0;
        playerSpeed = character.playerSpeed * 0.25f; 
        grounded = character.controller.isGrounded;
        gravityValue = character.gravityValue;
    }

    public override void HandleInput()
    {
        base.HandleInput();

        input = moveAction.ReadValue<Vector2>();
        velocity = new Vector3(input.x, 0, input.y);
        velocity = velocity.x * character.cameraTransform.right.normalized + velocity.z * character.cameraTransform.forward.normalized;
        velocity.y = 0f;


        if (!chargeManaAction.IsPressed() || character.healthSystem.IsHealthFull())
        {
            stateMachine.ChangeState(character.crouching);
        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();


        float manaToCost = manaCostPerSecond * Time.deltaTime;
        float healthToRegen = healthPerSecond * Time.deltaTime;

        if (character.healthSystem.TryUseMana(manaToCost))
        {
            character.healthSystem.RegenerateHealth(healthToRegen);
        }
        else
        {
            stateMachine.ChangeState(character.crouching);
        }

        character.animator.SetFloat("speed", input.magnitude, character.speedDampTime, Time.deltaTime);
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        gravityVelocity.y += gravityValue * Time.deltaTime;
        grounded = character.controller.isGrounded;
        if (grounded && gravityVelocity.y < 0)
        {
            gravityVelocity.y = 0f;
        }

        currentVelocity = Vector3.SmoothDamp(currentVelocity, velocity, ref cVelocity, character.velocityDampTime);
        character.controller.Move(currentVelocity * Time.deltaTime * playerSpeed + gravityVelocity * Time.deltaTime);

        if (velocity.sqrMagnitude > 0)
        {
            character.transform.rotation = Quaternion.Slerp(character.transform.rotation, Quaternion.LookRotation(velocity), character.rotationDampTime);
        }
    }

    public override void Exit()
    {
        base.Exit();

        
        if (activeVFX != null)
        {
            Object.Destroy(activeVFX);
        }
    }
}