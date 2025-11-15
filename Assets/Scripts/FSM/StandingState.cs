using UnityEngine;
 
public class StandingState: State
{  
    float gravityValue;
    bool jump;   
    bool crouch;
    Vector3 currentVelocity;
    bool grounded;
    float playerSpeed;
    bool drawWeapon;
    bool rangedAttack;
 
    Vector3 cVelocity;
 
    public StandingState(Character _character, StateMachine _stateMachine) : base(_character, _stateMachine)
    {
        character = _character;
        stateMachine = _stateMachine;
    }
 
    public override void Enter()
    {
        base.Enter();
        character.previousMovementState = this;

        jump = false;
        crouch = false;
        drawWeapon = false;
        rangedAttack = false;
        input = Vector2.zero;
        
        currentVelocity = Vector3.zero;
        gravityVelocity.y = 0;
 
        velocity = character.playerVelocity;
        playerSpeed = character.playerSpeed;
        grounded = character.controller.isGrounded;
        gravityValue = character.gravityValue;    
    }
 
    public override void HandleInput()
    {
        base.HandleInput();
 
        if (jumpAction.triggered)
        {
            jump = true;
        }
        if (crouchAction.triggered)
        {
            crouch = true;
        }

        input = moveAction.ReadValue<Vector2>();

 
        if (drawWeaponAction.triggered)
        {
            drawWeapon = true;
        }

        if (rangedAttackAction.triggered)
        {
            rangedAttack = true;
        }

 
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
        

       if (sprintAction.IsPressed() && input.magnitude > 0 && character.healthSystem.GetCurrentStamina() > 0)
        {
            stateMachine.ChangeState(character.sprinting);
            return; 
        }

        if (jump)
        {
            stateMachine.ChangeState(character.jumping);
        }
        if (crouch)
        {
            stateMachine.ChangeState(character.crouching);
        }
        if (drawWeapon)
        {
            stateMachine.ChangeState(character.combatting);
            character.animator.SetTrigger("drawWeapon");
        }
        if (rangedAttack)
        {
            character.animator.SetTrigger("cast1");
            stateMachine.ChangeState(character.casting);
        }
        
        if (chargeManaAction.triggered)
        {
            stateMachine.ChangeState(character.charging);
        }
    }
 
    public override void PhysicsUpdate(float speedModifier)
    {
        base.physicsUpdate(speedModifier);
 
        gravityVelocity.y += gravityValue * Time.deltaTime;
        grounded = character.controller.isGrounded;
 
        if (grounded && gravityVelocity.y < 0)
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
 
        gravityVelocity.y = 0f;
        character.playerVelocity = new Vector3(input.x, 0, input.y);
 
        if (velocity.sqrMagnitude > 0)
        {
            character.transform.rotation = Quaternion.LookRotation(velocity);
        }
    }
 
}