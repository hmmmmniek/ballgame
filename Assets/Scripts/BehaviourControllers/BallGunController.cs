using System;
using Fusion;
using UnityEngine;


public class BallGunController : NetworkBehaviour {
    [Header("Ball gun Settings")]
    public float maxShootingSpeed = 50;
    public float chargeSpeed = 100;
    public float dechargeSpeed = 400;
    public float suckDistance = 5;
    public float suckRadius = 5;
    public float suckTimeout = 0.5f;
    public float kickDistance = 5;
    public float kickRadius = 5;
    public Camera localBallCamera;

    [HideInInspector]
    public BallController ball;

    
    [Networked] private TickTimer suckTimer { get; set; }

    [Networked] private float chargePercentage { get; set; }

    public bool isLocalPlayer = false;



    protected void Awake() {
    }

    public override void Spawned() {
        if (Object.HasInputAuthority) {
            isLocalPlayer = true;
        } else {
            localBallCamera.gameObject.SetActive(false);
        }
        chargePercentage = 0;
        suckTimer = TickTimer.None;

    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);
    }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {
            if (networkInputData.isPrimaryPressed) {
                Charge();
            } else {
                if(chargePercentage > 0) {
                    Shoot();
                }
                Decharge();
            }
            if (networkInputData.isSecondaryPressed) {
                if(IsCarryingBall()) {
                    //Kick();
                } else if(suckTimer.ExpiredOrNotRunning(Runner)) {
                    Suck();
                }
            }
        }
        if(IsCarryingBall() && ball.transform.parent == null) {
            ball.AttachToPlayer(this);
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
        if(IsCarryingBall()) {
            Vector3 direction = transform.forward.normalized;
            ball.Shoot(direction * maxShootingSpeed * (chargePercentage/100f));
        }
    }

    public void Suck() {
        suckTimer = TickTimer.CreateFromSeconds(Runner, suckTimeout);
        if(ball != null && ball.attachedToPlayer == BallController.NOT_ATTACHED) {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, suckRadius, transform.forward, suckDistance);
            foreach (var hit in hits) {
                BallController b = hit.transform.GetComponent<BallController>();
                if(b) {
                    b.AttachToPlayer(this);
                }
            }
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

    public bool IsCarryingBall() {
        return ball!= null && ball.attachedToPlayer == this.Id.Object.Raw;
    }
}