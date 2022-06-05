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
    }

    private void HandleAction(InputAction.CallbackContext context) {
        if (context.action.name == "Jump") {
            networkInputDataCache.isJumpPressed = networkInputDataCache.isJumpPressed || context.ReadValueAsButton();
            latestJumpValue = context.ReadValueAsButton();
        }
        if (context.action.name == "Move") {
            networkInputDataCache.movementInput = context.ReadValue<Vector2>();
        }
        if (context.action.name == "Look") {
            networkInputDataCache.rotationInput += context.ReadValue<Vector2>() * new Vector2(InputHandler.instance.lookHorizontalSpeed, -1 * InputHandler.instance.lookVerticalSpeed);
            networkInputDataCache.rotationInput = new Vector2(networkInputDataCache.rotationInput.x, Mathf.Clamp(networkInputDataCache.rotationInput.y, -90, 90));
        }
        if (context.action.name == "Primary") {
            networkInputDataCache.isPrimaryPressed = networkInputDataCache.isPrimaryPressed || context.ReadValueAsButton();
            latestPrimaryValue = context.ReadValueAsButton();
        }
        if (context.action.name == "Secondary") {
            networkInputDataCache.isSecondaryPressed = networkInputDataCache.isSecondaryPressed || context.ReadValueAsButton();
            latestSecondaryValue = context.ReadValueAsButton();
        }
        if (context.action.name == "Escape") {
            StopGameInput();
            uiManager.ShowMenu();
        }
    }
    public void ResetNetworkState() {
        networkInputDataCache.isJumpPressed = latestJumpValue;
        networkInputDataCache.isPrimaryPressed = latestPrimaryValue;
        networkInputDataCache.isSecondaryPressed = latestSecondaryValue;

    }
}

