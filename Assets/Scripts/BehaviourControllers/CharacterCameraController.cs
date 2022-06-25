using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterCameraController : NetworkTransform {
    public Transform cameraAnchorPoint;
    public Transform bodyAnchorPoint;
    public PlayerController playerController;
    private bool isLocal = false;


    public override void Spawned() {    
        base.Spawned();
        transform.parent = null;
        isLocal = playerController.IsLocal();
    }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData) && !isLocal) {
            Rotate(networkInputData.rotationInput);
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
        }
       

    }

}

