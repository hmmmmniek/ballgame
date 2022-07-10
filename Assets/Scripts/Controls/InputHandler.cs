using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using System;

public class InputHandler : MonoBehaviour {
    public static InputHandler instance = null;
    public UIManager uiManager;
    public float lookHorizontalSpeed = 15.0f;
    public float lookVerticalSpeed = 15.0f;

    [HideInInspector]
    public NetworkInputData networkInputDataCache;

    public LocalInputData localInputDataCache;



    PlayerInput playerInput;

    private bool latestJumpValue;
    private bool latestPrimaryValue;
    private bool latestSecondaryValue;
    private void Awake() {
        InputHandler.instance = this;
        playerInput = GetComponent<PlayerInput>();


    }

    public void StartGameInput() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerInput.onActionTriggered += HandleAction;
    }

    public void StopGameInput() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        playerInput.onActionTriggered -= HandleAction;

        localInputDataCache.primaryPressed = false;
        localInputDataCache.secondaryPressed = false;
        localInputDataCache.jumpPressed = false;
        localInputDataCache.sprintPressed = false;
        localInputDataCache.ballSpinPressed = false;
        localInputDataCache.ballRollPressed = false;
    }

    private void HandleAction(InputAction.CallbackContext context) {
        if (context.action.name == "Jump") {
            if(context.ReadValueAsButton()) {
                localInputDataCache.jumpPressed = true;
            } else {
                localInputDataCache.jumpPressed = false;
            }
        }

        if (context.action.name == "Sprint") {
            if(context.ReadValueAsButton()) {
                localInputDataCache.sprintPressed = true;
            } else {
                localInputDataCache.sprintPressed = false;
            }
        }

        if (context.action.name == "Shield") {
            if(context.ReadValueAsButton()) {
                localInputDataCache.shieldPressed = true;
            } else {
                localInputDataCache.shieldPressed = false;
            }
        }

        if (context.action.name == "Ball spin") {
            if(context.ReadValueAsButton()) {
                localInputDataCache.ballSpinPressed = true;
            } else {
                localInputDataCache.ballSpinPressed = false;
            }
        }
        
        if (context.action.name == "Ball roll") {
            if(context.ReadValueAsButton()) {
                localInputDataCache.ballRollPressed = true;
            } else {
                localInputDataCache.ballRollPressed = false;
            }
        }
        
        if (context.action.name == "Move") {
            networkInputDataCache.movementInput = context.ReadValue<Vector2>();
        }
        if (context.action.name == "Look") {
            networkInputDataCache.rotationInput += context.ReadValue<Vector2>() * new Vector2(InputHandler.instance.lookHorizontalSpeed, -1 * InputHandler.instance.lookVerticalSpeed);
            networkInputDataCache.rotationInput = new Vector2(networkInputDataCache.rotationInput.x, Mathf.Clamp(networkInputDataCache.rotationInput.y, -90, 90));
        }
        if (context.action.name == "Primary") {
            if(context.ReadValueAsButton()) {
                localInputDataCache.primaryPressed = true;
            } else {
                localInputDataCache.primaryPressed = false;
            }
        }
        if (context.action.name == "Secondary") {
            if(context.ReadValueAsButton()) {
                localInputDataCache.secondaryPressed = true;
            } else {
                localInputDataCache.secondaryPressed = false;
            }
        }

        if (context.action.name == "Scoreboard") {
            if(context.ReadValueAsButton()) {
                InputState.Dispatch(InputState.SetShowScoreboard, true, () => {});
            } else {
                InputState.Dispatch(InputState.SetShowScoreboard, false, () => {});
            }
        }

        if (context.action.name == "Escape") {
            //StopGameInput();
            //uiManager.ShowMenu();
        }
    }
}


