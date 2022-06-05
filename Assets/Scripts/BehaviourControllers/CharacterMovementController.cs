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

    public CharacterCameraController cameraController;

    [Networked]
    [HideInInspector]
    public bool IsGrounded { get; set; }

    [Networked]
    [HideInInspector]
    public Vector3 Velocity { get; set; }

    [Networked]
    private float boostRemainingPercentage { get; set; }

    /// <summary>
    /// Sets the default teleport interpolation velocity to be the CC's current velocity.
    /// For more details on how this field is used, see <see cref="NetworkTransform.TeleportToPosition"/>.
    /// </summary>
    protected override Vector3 DefaultTeleportInterpolationVelocity => Velocity;

    /// <summary>
    /// Sets the default teleport interpolation angular velocity to be the CC's rotation speed on the Z axis.
    /// For more details on how this field is used, see <see cref="NetworkTransform.TeleportToRotation"/>.
    /// </summary>
    protected override Vector3 DefaultTeleportInterpolationAngularVelocity => new Vector3(0f, 0f, InputHandler.instance.lookHorizontalSpeed);

    public CharacterController Controller { get; private set; }

    private bool isLocalPlayer = false;

    protected override void Awake() {
        base.Awake();
        CacheController();
    }

    public override void Spawned() {
        base.Spawned();
        CacheController();
        if (Object.HasInputAuthority) {
            isLocalPlayer = true;
        }

        // Caveat: this is needed to initialize the Controller's state and avoid unwanted spikes in its perceived velocity
        Controller.Move(transform.position);
        boostRemainingPercentage = 100f;
    }

    private void CacheController() {
        if (Controller == null) {
            Controller = GetComponent<CharacterController>();

            Assert.Check(Controller != null, $"An object with {nameof(CharacterMovementController)} must also have a {nameof(CharacterController)} component.");
        }
    }

    protected override void CopyFromBufferToEngine() {
        // Trick: CC must be disabled before resetting the transform state
        Controller.enabled = false;

        // Pull base (NetworkTransform) state from networked data buffer
        base.CopyFromBufferToEngine();

        // Re-enable CC
        Controller.enabled = true;
    }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {
            if (networkInputData.isJumpPressed) {
                Jump();
            }
            Move(networkInputData.movementInput);

        }
        RechargeBoost();
        Rotate();
    }

    private void RechargeBoost() {
        if(IsGrounded && boostRemainingPercentage < 100) {
            boostRemainingPercentage = boostRemainingPercentage + boostRechargeSpeed * Runner.DeltaTime;
            if(boostRemainingPercentage > 100) {
                boostRemainingPercentage = 100;
            }
            if(isLocalPlayer) {
                GameState.Dispatch(GameState.SetRemainingBoostPercentage, boostRemainingPercentage, () => {});
            }
        }
    }

    public virtual void Jump(bool ignoreGrounded = false, float? overrideImpulse = null) {
        if (IsGrounded || ignoreGrounded) {
            var newVel = Velocity;
            newVel.y += overrideImpulse ?? jumpImpulse;
            Velocity = newVel;
        } else if(boostRemainingPercentage > 0) {
            var newVel = Velocity;
            newVel.y += boostImpulse * Runner.DeltaTime;
            Velocity = newVel;
            boostRemainingPercentage = boostRemainingPercentage - boostUsageSpeed * Runner.DeltaTime;
            if(boostRemainingPercentage < 0) {
                boostRemainingPercentage = 0;
            }
            if(isLocalPlayer) {
                GameState.Dispatch(GameState.SetRemainingBoostPercentage, boostRemainingPercentage, () => {});
            }
        }

    }

    public virtual void Move(Vector2 movementInput) {

        var deltaTime = Runner.DeltaTime;
        var previousPos = transform.position;
        var moveVelocity = Velocity;

        Vector3 direction = transform.forward * movementInput.y + transform.right * movementInput.x;
        direction = direction.normalized;

        if (IsGrounded && moveVelocity.y < 0) {
            moveVelocity.y = 0f;
        }

        moveVelocity.y += gravity * Runner.DeltaTime;

        var horizontalVel = default(Vector3);
        horizontalVel.x = moveVelocity.x;
        horizontalVel.z = moveVelocity.z;

        if (direction == default) {
            horizontalVel = Vector3.Lerp(horizontalVel, default, braking * deltaTime);
        } else {
            horizontalVel = Vector3.ClampMagnitude(horizontalVel + direction * acceleration * deltaTime, maxGroundSpeed);
        }

        moveVelocity.x = horizontalVel.x;
        moveVelocity.z = horizontalVel.z;
        if(moveVelocity.y > maxVerticalSpeed) {
            moveVelocity.y = maxVerticalSpeed;
        }
        Controller.Move(moveVelocity * deltaTime);

        Velocity = (transform.position - previousPos) * Runner.Simulation.Config.TickRate;
        IsGrounded = Controller.isGrounded;
    }

    public void Rotate() {
        transform.localRotation = Quaternion.Euler(0, cameraController.transform.localEulerAngles.y, 0f);

    }
}
