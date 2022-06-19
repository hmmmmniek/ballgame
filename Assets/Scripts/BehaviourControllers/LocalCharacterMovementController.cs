using System;
using Fusion;
using UnityEngine;

public class LocalCharacterMovementController : MonoBehaviour {
    public Transform bodyAnchorPoint;
    public CharacterCameraController cameraController;
    public CharacterMovementController networkMovementController;
    private CharacterController Controller;

    public Vector3 Velocity;
    public float boostRemainingPercentage;
    public bool hasInputAuthority;

    private bool _localJump;
    public bool localJump { get { return _localJump; } set { _localJump = value; } } 

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
    public void Update() {
        
        /*
        * Move model to position when not in control
        */
        if(!hasInputAuthority) {
            transform.position = bodyAnchorPoint.position;
            return;
        }
        
        float deltaTime = Time.deltaTime;


        /*
        * Accept/refuse server state
        */
        if( 
            Vector3.Distance(transform.position, networkMovementController.transform.position) > networkMovementController.maxAllowedClientPositionError ||
            Vector3.Distance(Velocity, networkMovementController.Velocity) > networkMovementController.maxAllowedClientVelocityError ||
            Math.Abs(boostRemainingPercentage - networkMovementController.boostRemainingPercentage) > networkMovementController.maxAllowedClientBoostError
        ) {
            Debug.Log("synchronize");
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
            Controller.isGrounded
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
        Velocity = Utils.Move(
            deltaTime,
            InputHandler.instance.networkInputDataCache.movementInput,
            transform,
            Velocity,
            Controller,
            networkMovementController.gravity,
            networkMovementController.braking,
            networkMovementController.acceleration,
            networkMovementController.maxGroundSpeed,
            networkMovementController.maxVerticalSpeed
        );

        /*
        * Rotate
        */
        transform.rotation = Quaternion.Euler(0, InputHandler.instance.networkInputDataCache.rotationInput.x, 0);

        
    }

    public void OnDestroy (){
        GameObject.Destroy(cameraController.gameObject);
    }

}
