using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

public class InputHandler : NetworkBehaviour
{
    [HideInInspector]
    public NetworkInputData networkInputDataCache;

    PlayerInput playerInput;

    CharacterMovementController characterMovementController;
    CharacterUpDownController characterUpDownController;
    BallGunController ballGunController;
    private void Awake() {
        playerInput = GetComponent<PlayerInput>();

        characterMovementController = GetComponent<CharacterMovementController>();
        characterUpDownController = GetComponentInChildren<CharacterUpDownController>();
        ballGunController = GetComponentInChildren<BallGunController>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerInput.onActionTriggered += HandleAction;
    }

    private void HandleAction(InputAction.CallbackContext context)
    {
        if(context.action.name == "Jump") {
            networkInputDataCache.isJumpPressed = context.ReadValueAsButton();
        }
        if(context.action.name == "Move") {
            networkInputDataCache.movementInput = context.ReadValue<Vector2>();
        }
        if(context.action.name == "Look") {
            networkInputDataCache.rotationInput = context.ReadValue<Vector2>() * new Vector2(1, -1);
        }
        if(context.action.name == "Fire") {
            networkInputDataCache.isFirePressed = context.ReadValueAsButton();
        }
    }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {

            characterMovementController.Rotate(networkInputData.rotationInput);
            characterUpDownController.Rotate(networkInputData.rotationInput);

            characterMovementController.Move(networkInputData.movementInput);

            if (networkInputData.isJumpPressed) {
                characterMovementController.Jump();
            }

            if (networkInputData.isFirePressed) {
                ballGunController.Shoot();
            }
        }
    }

}

