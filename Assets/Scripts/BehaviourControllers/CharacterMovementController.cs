using System;
using Fusion;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[OrderBefore(typeof(NetworkTransform))]
[DisallowMultipleComponent]
// ReSharper disable once CheckNamespace
public class CharacterMovementController : NetworkTransform {
    [Header("Character movement Settings")]
    public float gravity = -20.0f;
    public float jumpImpulse = 5.0f;
    public float boostImpulse = 25.0f;
    public float acceleration = 10.0f;
    public float braking = 10.0f;
    public float maxGroundSpeed = 2.0f;
    public float maxVerticalSpeed = 6f;
    public float boostUsageSpeed = 40f;
    public float boostRechargeSpeed = 15f;
    public float rememberInputTime = 0.5f;
    public float maxAllowedClientPositionError = 1f;
    public float maxAllowedClientVelocityError = 1f;
    public float maxAllowedClientBoostError = 1f;

    public CharacterCameraController cameraController;
    public LocalCharacterMovementController localCharacterMovementController;


    [HideInInspector][Networked]public Vector3 Velocity { get; set; }
    [HideInInspector][Networked(OnChanged = nameof(OnBoostChanged))]
    public float boostRemainingPercentage { get; set; }
    public static void OnBoostChanged(Changed<CharacterMovementController> changed) {
        changed.Behaviour.OnBoostChanged();
    }
    private void OnBoostChanged() {
        if(Object.HasInputAuthority) {
            GameState.Dispatch(GameState.SetRemainingBoostPercentage, boostRemainingPercentage, () => {});
        }
    }

    
    [HideInInspector][Networked(OnChanged = nameof(OnJumpReceived))] public bool jumpReceived { get; set; }
    public static void OnJumpReceived(Changed<CharacterMovementController> changed) {
        changed.Behaviour.OnJumpReceived();
    }
    private void OnJumpReceived() {
        if(jumpReceived && Object.HasInputAuthority && !InputHandler.instance.localInputDataCache.jumpPressed) {
            localCharacterMovementController.localJump = false;
        }
    }

    protected override Vector3 DefaultTeleportInterpolationVelocity => Velocity;
    protected override Vector3 DefaultTeleportInterpolationAngularVelocity => new Vector3(0f, 0f, InputHandler.instance.lookHorizontalSpeed);
    public CharacterController Controller { get; private set; }

    public void Start() {
    }

    protected override void Awake() {
        base.Awake();
        CacheController();
    }

    public override void Spawned() {
        base.Spawned();
        CacheController();

       // Controller.Move(transform.position);
        boostRemainingPercentage = 100f;

        Physics.IgnoreCollision(localCharacterMovementController.GetComponent<CharacterController>(), GetComponent<CharacterController>());

        localCharacterMovementController.Init(Object.HasInputAuthority);
        localCharacterMovementController.Synchronize();
    }

    private void CacheController() {
        if (Controller == null) {
            Controller = GetComponent<CharacterController>();

            Assert.Check(Controller != null, $"An object with {nameof(CharacterMovementController)} must also have a {nameof(CharacterController)} component.");
        }
    }

    protected override void CopyFromBufferToEngine() {
        Controller.enabled = false;
        base.CopyFromBufferToEngine();
        Controller.enabled = true;
    }

    private Vector2 movement;
    private bool clientJump;
    private Vector3 clientPosition;
    private Vector3 clientVelocity;
    private float clientBoostRemaining;

    private bool previousClientJump = false;
    public override void FixedUpdateNetwork() {

        if(Object.HasStateAuthority) {
            bool receivedInput = false;
            if (GetInput(out NetworkInputData networkInputData)) {
                clientJump = networkInputData.clientJump;
                movement = networkInputData.movementInput;
                clientPosition = networkInputData.clientPosition;
                clientVelocity = networkInputData.clientVelocity;
                clientBoostRemaining = networkInputData.clientBoostRemaining;
                receivedInput = true;
            }
            float delta = Runner.DeltaTime;


            /*
            * Jump
            */
            if(
                clientJump &&
                Controller.isGrounded
            ) {
                Velocity = Utils.Jump(
                    delta,
                    Velocity,
                    Controller,
                    jumpImpulse
                );
                jumpReceived = true;
            }

            /*
            * Reset jump
            */
            if(
                previousClientJump == true &&
                !clientJump
            ) {
                jumpReceived = false;
            }

            /*
            * Boost
            */
            if(
                clientJump &&
                !Controller.isGrounded
            ) {
                (float b, Vector3 v) = Utils.Boost(
                    delta,
                    Velocity,
                    Controller,
                    boostImpulse,
                    boostUsageSpeed,
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
                    delta,
                    Controller,
                    boostRemainingPercentage,
                    boostRechargeSpeed
                );
            }
                        


            /*
            * Move
            */
            Velocity = Utils.Move(
                delta,
                movement,
                transform,
                Velocity,
                Controller,
                gravity,
                braking,
                acceleration,
                maxGroundSpeed,
                maxVerticalSpeed
            );

            /*
            * Accept/refuse client state
            */
            if(
                receivedInput && 
                Vector3.Distance(clientPosition, transform.position) < maxAllowedClientPositionError &&
                Vector3.Distance(clientVelocity, Velocity) < maxAllowedClientVelocityError &&
                Math.Abs(boostRemainingPercentage - clientBoostRemaining) < maxAllowedClientBoostError
            ) {
                Velocity = clientVelocity;
                boostRemainingPercentage = clientBoostRemaining;
                Controller.enabled = false;
                transform.position = clientPosition;
                Controller.enabled = true;
            }

        }
    }

    public void Push(Vector3 force) {
        Velocity += force;
    }

    public void Update() {
        Utils.Rotate(transform, cameraController.transform.localEulerAngles.y);
    }


    public void OnDestroy (){
        GameObject.Destroy(localCharacterMovementController.gameObject);
    }

}
