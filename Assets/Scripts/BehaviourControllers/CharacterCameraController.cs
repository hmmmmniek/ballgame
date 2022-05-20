using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterCameraController : NetworkTransform {
    public Transform cameraAnchorPoint;
    public PlayerController playerController;
    private bool isLocal = false;


    void Start() {
        transform.parent = null;
        isLocal = PlayerController.Local == playerController;
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
        transform.rotation = Quaternion.Euler(viewInput.y, viewInput.x, 0);
    }

}
