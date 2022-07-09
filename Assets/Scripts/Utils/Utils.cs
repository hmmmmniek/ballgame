using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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


    public static (float boost, Vector3 velocity) Dash(
        CharacterController controller,
        Vector2 movementInput,
        Transform transform,
        float dashImpulse,
        float dashBoostUsage,
        float boostRemainingPercentage
    ) {
        if(boostRemainingPercentage < dashBoostUsage || movementInput.magnitude == 0) {
            return (boost: boostRemainingPercentage, velocity: new Vector3());
        }
        Vector3 direction = transform.forward * movementInput.y + transform.right * movementInput.x;
        direction = direction.normalized;

        Vector3 newVelocity = direction * dashImpulse;

        return (boost: boostRemainingPercentage - dashBoostUsage, velocity: newVelocity);
        
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


    public static (float boost, Vector3 velocity) Move(
        float deltaTime,
        Vector2 movementInput,
        Transform transform,
        Vector3 velocity,
        CharacterController controller,
        float boostRemainingPercentage,
        float gravity,
        float braking,
        float acceleration,
        float maxBallWalkingSpeed,
        float maxSprintGroundSpeed,
        float maxGroundSpeed,
        float maxVerticalSpeed,
        bool isSprinting,
        float boostUsageSpeed,
        bool isCarrying
    ) {

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

        Vector3 x = horizontalVel + direction * acceleration * deltaTime;

        float maximumGroundSpeed = isCarrying ? maxBallWalkingSpeed : ((isSprinting && boostRemainingPercentage > 0) ? maxSprintGroundSpeed : maxGroundSpeed);

        if(controller.isGrounded) {
            if (direction == default) {
                horizontalVel = Vector3.Lerp(horizontalVel, default, braking * deltaTime);
            } else {
                horizontalVel = Vector3.ClampMagnitude(x, maximumGroundSpeed);
            }
        } else {
            float groundSpeed = new Vector2(velocity.x, velocity.z).magnitude;
            if(groundSpeed > maximumGroundSpeed) {
                horizontalVel = Vector3.ClampMagnitude(x, groundSpeed);
            } else {
                horizontalVel = Vector3.ClampMagnitude(x, maximumGroundSpeed);
            }
        }


        moveVelocity.x = horizontalVel.x;
        moveVelocity.z = horizontalVel.z;
        if(moveVelocity.y > maxVerticalSpeed) {
            moveVelocity.y = maxVerticalSpeed;
        }
        controller.Move(moveVelocity * deltaTime);
        velocity = (transform.position - previousPos) / deltaTime;

        if(!isCarrying && isSprinting && boostRemainingPercentage > 0 && movementInput.magnitude > 0) {
            boostRemainingPercentage = boostRemainingPercentage - boostUsageSpeed * deltaTime;
            if(boostRemainingPercentage < 0) {
                boostRemainingPercentage = 0;
            }

        }

        return (boost: boostRemainingPercentage, velocity: velocity);
    }
    public static Vector3 PushToOwnHalf(Transform transform, float deltaTime, Team team, Vector3 velocity, Vector3 ballPos, CharacterController controller, float maxSprintGroundSpeed, float middleCircleRadius) {
        var previousPos = transform.position;

        Vector3 ballToPlayer = new Vector3(previousPos.x, 0, previousPos.z) - new Vector3(ballPos.x, 0, ballPos.z);
        bool pushed = false;
        switch(team) {
            case Team.Blue: {
                if(transform.position.z > 0) {
                    controller.Move(new Vector3(0, 0, -1) * maxSprintGroundSpeed * deltaTime);
                    pushed = true;
                }
                break;
            }
            case Team.Red: {
                if(transform.position.z < 0) {
                    controller.Move(new Vector3(0, 0, 1) * maxSprintGroundSpeed * deltaTime);
                    pushed = true;
                }
                break;
            }
        }
        if(!pushed && ballToPlayer.magnitude < middleCircleRadius) {
            controller.Move(ballToPlayer.normalized * maxSprintGroundSpeed * deltaTime);
        }
        velocity = (transform.position - previousPos) / deltaTime;
        return velocity;
    }

    public static void Rotate(Transform transform, float eulerAngles) {
        transform.localRotation = Quaternion.Euler(0, eulerAngles, 0f);
    }

    public static float RechargeBoost(float deltaTime, CharacterController controller, float boostRemainingPercentage, float boostRechargeSpeed) {
        if(boostRemainingPercentage < 100) {
            boostRemainingPercentage = boostRemainingPercentage + boostRechargeSpeed * deltaTime;
            if(boostRemainingPercentage > 100) {
                boostRemainingPercentage = 100;
            }
        }
        return boostRemainingPercentage;
    }

    public static string GetCurrentProcessId() {
        Process currentProcess = Process.GetCurrentProcess();
        string pid = currentProcess.Id.ToString();
        return SystemInfo.deviceUniqueIdentifier + pid;
    }
}
