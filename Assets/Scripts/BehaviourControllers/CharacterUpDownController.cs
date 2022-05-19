using System;
using Fusion;
using UnityEngine;

public class CharacterUpDownController : NetworkTransform {
    [Header("Character up/down Settings")]

    public float lookVerticalSpeed = 15.0f;

    float cameraRotationX {get; set;}
    public Camera localCamera {get; private set;}

    protected override void Awake() {
        base.Awake();
        CacheCamera();
    }

    public override void Spawned() {
        base.Spawned();
        CacheCamera();
    }


    private void CacheCamera() {
        if (localCamera == null) {
            localCamera = GetComponentInChildren<Camera>();

            Assert.Check(localCamera != null, $"An object with {nameof(CharacterUpDownController)} must also have a {nameof(Camera)} component in its children.");
        }
    }

    public void Rotate(Vector2 rotationInput) {
        cameraRotationX += rotationInput.y * Runner.DeltaTime * lookVerticalSpeed;
        cameraRotationX = Mathf.Clamp(cameraRotationX, -90, 90);

        localCamera.transform.localRotation = Quaternion.Euler(cameraRotationX, 0, 0);
    }
}
