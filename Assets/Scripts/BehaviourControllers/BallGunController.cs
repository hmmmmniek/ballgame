using System;
using Fusion;
using UnityEngine;


public class BallGunController : NetworkBehaviour {
    [Header("Ball gun Settings")]
    public float speed = 5;
    public BallController ballPrefab;

    public Camera localCamera {get; private set;}
    [Networked] private TickTimer delay { get; set; }

    protected void Awake() {
        CacheCamera();
    }

    private void CacheCamera() {
        if (localCamera == null) {
            localCamera = GetComponentInChildren<Camera>();

            Assert.Check(localCamera != null, $"An object with {nameof(CharacterUpDownController)} must also have a {nameof(Camera)} component in its children.");
        }
    }

    public void Shoot() {
        if (delay.ExpiredOrNotRunning(Runner)) {
            delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

            Vector3 direction = localCamera.transform.forward.normalized;
            Runner.Spawn(ballPrefab, transform.position + (direction), Quaternion.LookRotation(direction), Object.InputAuthority, (runner, o) => {
                o.GetComponent<BallController>().Init(direction * speed);
            });
        }
    }
}