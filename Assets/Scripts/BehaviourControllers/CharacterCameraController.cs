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

    private bool isLocal = false;


    public override void Spawned() {    
        base.Spawned();
        transform.parent = null;

    }

    public void Init(PlayerRef inputAuthority) {
        isLocal = playerController.IsLocal();
        transform.name = $"#{inputAuthority.PlayerId} Camera";

    }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {
            if(!isLocal) {
                Rotate(networkInputData.rotationInput);
            }

        }
    }

    void Update() {
        transform.position = cameraAnchorPoint.position;

        if(isLocal) {
            Rotate(InputHandler.instance.networkInputDataCache.rotationInput);
        }
    }


    void Rotate(Vector2 viewInput) {
        if(!playerController.knockedOut) {
            transform.rotation = Quaternion.Euler(viewInput.y, viewInput.x, 0);
            playerController.bodyMeshRenderer.material.SetFloat("_Rotation", viewInput.y / 90f);
            ballAnchorWrapper.localRotation = Quaternion.Euler(viewInput.y * (viewInput.y > 0 ? ballAnchorPositiveRotationMultiplier : ballAnchorNegativeRotationMultiplier), 0, 0);
        }
       

    }

}
