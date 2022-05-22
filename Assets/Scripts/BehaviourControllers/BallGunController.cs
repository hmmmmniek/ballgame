using System;
using Fusion;
using UnityEngine;


public class BallGunController : NetworkBehaviour {
    [Header("Ball gun Settings")]
    public float speed = 5;
    public BallController ballPrefab;

    [Networked] private TickTimer delay { get; set; }

    protected void Awake() {
    }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {
            if (networkInputData.isFirePressed) {
                Shoot();
            }
        }
    }

    public void Shoot() {
        if (delay.ExpiredOrNotRunning(Runner)) {
            delay = TickTimer.CreateFromSeconds(Runner, 0.5f);

            Vector3 direction = transform.forward.normalized;
            Runner.Spawn(ballPrefab, transform.position + (direction), Quaternion.LookRotation(direction), null, (runner, o) => {
                o.GetComponent<BallController>().Init(direction * speed);
            });
        }
    }
}