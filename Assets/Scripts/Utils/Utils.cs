using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils {
    public static Vector3 GetRandomSpawnPoint() {
        return new Vector3(Random.Range(-5, 5), 4, Random.Range(-5, 5));
    }
    public static void SetRenderLayerDeep(Transform transform, int layerNumber) {
        transform.gameObject.layer = layerNumber;
        foreach (Transform trans in transform.GetComponentsInChildren<Transform>(true)) {
            trans.gameObject.layer = layerNumber;
        }
    }



    public static Vector3 Jump(float deltaTime, Vector3 velocity, CharacterController controller, float jumpImpulse) {
        if (controller.isGrounded) {
            var newVel = velocity;
            newVel.y += jumpImpulse;
            velocity = newVel;
        }
        return velocity;
    }

    public static (float, Vector3) Boost(float deltaTime, Vector3 velocity, CharacterController controller, float boostImpulse, float boostUsageSpeed, float boostRemainingPercentage) {
        if(boostRemainingPercentage > 0) {
            var newVel = velocity;
            newVel.y += boostImpulse * deltaTime;
            velocity = newVel;
            boostRemainingPercentage = boostRemainingPercentage - boostUsageSpeed * deltaTime;
            if(boostRemainingPercentage < 0) {
                boostRemainingPercentage = 0;
            }

        }
        return (boostRemainingPercentage, velocity);

    }
    public static Vector3 Move(float deltaTime, Vector2 movementInput, Transform transform, Vector3 velocity, CharacterController controller, float gravity, float braking, float acceleration, float maxGroundSpeed, float maxVerticalSpeed) {

        var previousPos = transform.position;
        var moveVelocity = velocity;

        Vector3 direction = transform.forward * movementInput.y + transform.right * movementInput.x;
        direction = direction.normalized;

        if (controller.isGrounded && moveVelocity.y < 0) {
            moveVelocity.y = 0f;
        }

        moveVelocity.y += gravity * deltaTime;

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
        controller.Move(moveVelocity * deltaTime);
        velocity = (transform.position - previousPos) / deltaTime;
        return velocity;
    }

    public static void Rotate(Transform transform, float eulerAngles) {
        transform.localRotation = Quaternion.Euler(0, eulerAngles, 0f);
    }

    public static float RechargeBoost(float deltaTime, CharacterController controller, float boostRemainingPercentage, float boostRechargeSpeed) {
        if(controller.isGrounded && boostRemainingPercentage < 100) {
            boostRemainingPercentage = boostRemainingPercentage + boostRechargeSpeed * deltaTime;
            if(boostRemainingPercentage > 100) {
                boostRemainingPercentage = 100;
            }
        }
        return boostRemainingPercentage;
    }
}
