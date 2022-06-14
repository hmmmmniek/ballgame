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
        

        if(!hasInputAuthority) {
            transform.position = bodyAnchorPoint.position;
            return;
        }
        float deltaTime = Time.deltaTime;
        if( 
            Vector3.Distance(transform.position, networkMovementController.transform.position) > networkMovementController.maxAllowedClientPositionError ||
            Vector3.Distance(Velocity, networkMovementController.Velocity) > networkMovementController.maxAllowedClientVelocityError ||
            Math.Abs(boostRemainingPercentage - networkMovementController.boostRemainingPercentage) > networkMovementController.maxAllowedClientBoostError
        ) {
            Synchronize();
        }
        if(InputHandler.instance.networkInputDataCache.jumpPressedTime + deltaTime - networkMovementController.Runner.SimulationTime > 0) {
            Velocity = Utils.Jump(
                deltaTime,
                Velocity,
                Controller,
                networkMovementController.jumpImpulse
            );
        }


        if(InputHandler.instance.networkInputDataCache.jumpPressedTime < InputHandler.instance.networkInputDataCache.runnerTime && InputHandler.instance.networkInputDataCache.jumpReleaseTime < InputHandler.instance.networkInputDataCache.jumpPressedTime) {
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

        boostRemainingPercentage = Utils.RechargeBoost(
            deltaTime,
            Controller,
            boostRemainingPercentage,
            networkMovementController.boostRechargeSpeed
        );
        transform.rotation = Quaternion.Euler(0, InputHandler.instance.networkInputDataCache.rotationInput.x, 0);

        
    }


    public void OnDestroy (){
        GameObject.Destroy(cameraController.gameObject);
    }

}
