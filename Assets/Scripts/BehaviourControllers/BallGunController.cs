using System;
using Fusion;
using UnityEngine;


public class BallGunController : NetworkBehaviour {
    [Header("Ball gun Settings")]
    public float maxShootingSpeed = 50;
    public float chargeSpeed = 100;
    public float dechargeSpeed = 400;
    public Camera localBallCamera;

    [HideInInspector]
    public BallController ball;

    

    [Networked]
    private float chargePercentage { get; set; }

    private bool isLocalPlayer = false;



    protected void Awake() {
    }

    public override void Spawned() {
        if (Object.HasInputAuthority) {
            isLocalPlayer = true;
        } else {
            localBallCamera.gameObject.SetActive(false);
        }
        chargePercentage = 0;

    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);
    }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {
            if (networkInputData.isFirePressed) {
                Charge();
            } else {
                if(chargePercentage > 0) {
                    Shoot();
                }
                Decharge();
            }
        }
    }

    public virtual void Charge() {
        if(chargePercentage < 100) {
            chargePercentage = chargePercentage + chargeSpeed * Runner.DeltaTime;
            if(chargePercentage > 100) {
                chargePercentage = 100;
            }
            if(isLocalPlayer) {
                GameState.Dispatch(GameState.SetChargePercentage, chargePercentage, () => {});
            }
        }

    }

    public virtual void Decharge() {
        if(chargePercentage > 0) {
            chargePercentage = chargePercentage - dechargeSpeed * Runner.DeltaTime;
            if(chargePercentage < 0) {
                chargePercentage = 0;
            }
            if(isLocalPlayer) {
                GameState.Dispatch(GameState.SetChargePercentage, chargePercentage, () => {});
            }
        }

    }
    public void Shoot() {
        if(ball!= null && ball.attachedToPlayer == this.Id.Object.Raw) {
            Vector3 direction = transform.forward.normalized;
            ball.Shoot(direction * maxShootingSpeed * (chargePercentage/100f));
        }
    }

    public void ShowBall() {
        localBallCamera.enabled = true;
        GameState.Dispatch(GameState.SetCarryingBall, true, () => {});
    }
    
    public void HideBall() {
        localBallCamera.enabled = false;
        GameState.Dispatch(GameState.SetCarryingBall, false, () => {});
    }
}