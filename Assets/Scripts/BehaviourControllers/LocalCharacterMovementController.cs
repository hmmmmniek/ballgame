using System;
using Fusion;
using UnityEngine;

public class LocalCharacterMovementController : MonoBehaviour {
    public CharacterCameraController cameraController;
    public CharacterMovementController networkMovementController;
    public CharacterController Controller;

    public Vector3 Velocity;
    public float boostRemainingPercentage;
    public bool hasInputAuthority;

    private float lastJumpTime;

    private bool _localJump;
    public bool localJump { get { return _localJump; } set { _localJump = value; } } 
    private bool _localSprint;
    public bool localSprint { get { return _localSprint; } set { _localSprint = value; } } 
    private bool _localDash;
    public bool localDash { get { return _localDash; } set { _localDash = value; } } 
    private bool _localHitGround;
    public bool localHitGround { get { return _localHitGround; } set { _localHitGround = value; } } 

    public void Init(bool hasInputAuthority) {
        this.hasInputAuthority = hasInputAuthority;
        transform.parent = null;
        Controller = GetComponent<CharacterController>();
        Physics.IgnoreCollision(GetComponent<CharacterController>(), networkMovementController.GetComponent<CharacterController>());

        if(!hasInputAuthority) {
            Controller.enabled = false;
        }
    }

    public void Synchronize() {
        boostRemainingPercentage = networkMovementController.boostRemainingPercentage;
        Controller.enabled = false;
        transform.position = networkMovementController.transform.position;
        Controller.enabled = true;
        Velocity = networkMovementController.Velocity;

    }

    private bool lastIsGrounded = false;
    public void Update() {

        /*
        * Dont do anything until object has been initialized
        */
        if(transform.parent != null) {
            return;
        }

        /*
        * Dont move model to position when not in control
        */
        if(!hasInputAuthority) {
            return;
        }
        
        float deltaTime = Time.deltaTime;


        /*
        * Accept/refuse server state
        */
        if( 
            !localDash && !localHitGround && (
                Vector3.Distance(transform.position, networkMovementController.transform.position) > networkMovementController.maxAllowedClientPositionError ||
                Vector3.Distance(Velocity, networkMovementController.Velocity) > networkMovementController.maxAllowedClientVelocityError ||
                Math.Abs(boostRemainingPercentage - networkMovementController.boostRemainingPercentage) > networkMovementController.maxAllowedClientBoostError
            )
        ) {
            Synchronize();
        }

        /*
        * Jump
        */
        if(
            InputHandler.instance.localInputDataCache.jumpPressed &&
            Controller.isGrounded
        ) {
            Velocity = Utils.Jump(
                deltaTime,
                Velocity,
                Controller,
                networkMovementController.jumpImpulse
            );
            localJump = true;
            lastJumpTime = Time.time;
        }

        /*
        * Stop jump
        */
        if (
            !InputHandler.instance.localInputDataCache.jumpPressed &&
            localJump &&
            networkMovementController.jumpReceived
        ) {   
            localJump = false;
        }

        /*
        * Dash
        */
        if(
            localJump == false &&
            InputHandler.instance.localInputDataCache.jumpPressed &&
            Time.time - lastJumpTime < networkMovementController.dashTimeout &&
            boostRemainingPercentage >= networkMovementController.dashBoostUsage &&
            !networkMovementController.ballGunController.isCarrying
        ) {
            lastJumpTime = 0;
            (float b, Vector3 v) = Utils.Dash(
                Controller,
                InputHandler.instance.networkInputDataCache.movementInput,
                transform,
                networkMovementController.dashImpulse,
                networkMovementController.dashBoostUsage,
                boostRemainingPercentage
            );
            boostRemainingPercentage = b;
            if(v.magnitude > 0) {
                Velocity = v;
            }

            localDash = true;
        }

        /*
        * Stop dash
        */
        if (
            !InputHandler.instance.localInputDataCache.jumpPressed &&
            localDash &&
            networkMovementController.dashReceived
        ) {   
            localDash = false;
        }


        /*
        * Boost
        */
        if(
            InputHandler.instance.localInputDataCache.jumpPressed &&
            !Controller.isGrounded
        ) {
            (float b, Vector3 v) = Utils.Boost(
                deltaTime,
                Velocity,
                Controller,
                networkMovementController.boostImpulse,
                networkMovementController.boostUsageSpeed,
                boostRemainingPercentage
            );
            boostRemainingPercentage = b;
            Velocity = v;
        }

        /*
        * Recharge boost
        */
        if(
            Controller.isGrounded &&
            (!InputHandler.instance.localInputDataCache.sprintPressed || InputHandler.instance.networkInputDataCache.movementInput.magnitude == 0 || networkMovementController.ballGunController.isCarrying)
        ) {
            boostRemainingPercentage = Utils.RechargeBoost(
                deltaTime,
                Controller,
                boostRemainingPercentage,
                networkMovementController.boostRechargeSpeed
            );
        }

        /*
        * Move
        */
    
        (float b2, Vector3 v2) = Utils.Move(
            deltaTime,
            InputHandler.instance.networkInputDataCache.movementInput,
            transform,
            Velocity,
            Controller,
            boostRemainingPercentage,
            networkMovementController.gravity,
            networkMovementController.braking,
            networkMovementController.acceleration,
            networkMovementController.maxBallWalkingSpeed,
            networkMovementController.maxSprintGroundSpeed,
            networkMovementController.maxGroundSpeed,
            networkMovementController.maxVerticalSpeed,
            InputHandler.instance.localInputDataCache.sprintPressed,
            networkMovementController.boostUsageSpeed,
            networkMovementController.ballGunController.isCarrying
        );
        Velocity = v2;
        boostRemainingPercentage = b2;
        /*
        * Rotate
        */
       
        transform.rotation = Quaternion.Euler(0, InputHandler.instance.networkInputDataCache.rotationInput.x, 0);
        
        

        
        
        /*
        * Grounded
        */
        if(
            Controller.isGrounded &&
            !lastIsGrounded
        ) {
            localHitGround = true;
        }
        
        if (
            localHitGround &&
            networkMovementController.hitGroundReceived
        ) {   
            localHitGround = false;
        }


        lastIsGrounded = Controller.isGrounded;
    }


}
