using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterCameraController : NetworkTransform {
    public Transform cameraAnchorPoint;
    public Transform bodyAnchorPoint;
    public PlayerController playerController;
    public Camera cam;
    public AudioListener audioListener;
    public Transform ballAnchorWrapper;
    public float ballAnchorPositiveRotationMultiplier;
    public float ballAnchorNegativeRotationMultiplier;

    [Networked(OnChanged = nameof(OnModelRotationChanged))] private float modelRotation { get; set; }
    public static void OnModelRotationChanged(Changed<CharacterCameraController> changed) {
        changed.Behaviour.OnModelRotationChanged();
    }
    private void OnModelRotationChanged() {
        if(Runner.LocalPlayer.PlayerId != Object.InputAuthority.PlayerId) {
            playerController.bodyMeshRenderer.material.SetFloat("_Rotation", modelRotation);
        }
    }

    public override void Spawned() {    
        base.Spawned();
        transform.parent = null;

    }

    public void Init(PlayerRef inputAuthority) {
        transform.name = $"#{(inputAuthority != null ? inputAuthority.PlayerId : "?")} Camera";
    }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {
            if(Runner.LocalPlayer.PlayerId != Object.InputAuthority.PlayerId) {
                Rotate(networkInputData.rotationInput);
            }
            modelRotation = networkInputData.rotationInput.y / 90f;
        }
    }

    void Update() {
        transform.position = cameraAnchorPoint.position;

        if(Runner.LocalPlayer.PlayerId == Object.InputAuthority.PlayerId) {
            Rotate(InputHandler.instance.networkInputDataCache.rotationInput);
        }
    }


    void Rotate(Vector2 viewInput) {
        if(!playerController.knockedOut) {
            transform.rotation = Quaternion.Euler(viewInput.y, viewInput.x, 0);
            ballAnchorWrapper.localRotation = Quaternion.Euler(viewInput.y * (viewInput.y > 0 ? ballAnchorPositiveRotationMultiplier : ballAnchorNegativeRotationMultiplier), 0, 0);
        }

    }

}
