using System;
using Fusion;
using UnityEngine;


public class BallGunController : NetworkBehaviour {
    [Header("Ball gun Settings")]
    public float maxShootingSpeed = 50;
    public float passSpeed = 30;
    public float chargeSpeed = 100;

    public float suckDistance = 2;
    public float suckRadius = 1.5f;
    public float suckTimeout = 0.5f;
    public float suckOffset = 1.5f;

    public float attractDistance = 1;
    public float attractRadius = 2.5f;
    public float attractWaitTime = 0.5f;
    public float attractOffset = 2.5f;
    public float attractStrength = 20f;

    public float kickDistance = 1f;
    public float kickRadius = 2.5f;
    public float kickTimeout = 0.5f;
    public float kickOffset = 2.5f;
    public float kickBallDropSpeed = 4;
    public float kickBallSpeed = 20;

    public Camera localBallCamera;
    public PlayerController playerController;
    public NetworkTransform ballAnchor;
    public Transform ballModel;

    [HideInInspector]
    public BallController ball;

    
    [Networked] private TickTimer suckTimer { get; set; }
    [Networked] private TickTimer attractTimer { get; set; }
    [Networked] private TickTimer kickTimer { get; set; }

    [Networked] private float chargePercentage { get; set; }
    [HideInInspector][Networked] public bool isCarrying { get; set; }

    [HideInInspector] public bool isLocalPlayer = false;
    
    
    
    private bool isSucking;
    private bool isKicking;

    protected void Awake() {
    }

 
    public override void Spawned() {
        if (Object.HasInputAuthority) {
            isLocalPlayer = true;
        } else {
            localBallCamera.gameObject.SetActive(false);
        }
        chargePercentage = 0;
        suckTimer = TickTimer.CreateFromSeconds(Runner, 0);
        kickTimer = TickTimer.CreateFromSeconds(Runner, 0);

    }
    private float lastButtonChange;

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);
    }

    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {

            
            if (networkInputData.isPrimaryPressed) {   

                if(isCarrying) {
                    Charge();
                }

                if(!isCarrying && kickTimer.Expired(Runner)) {
                    kickTimer = TickTimer.None;
                    Kick();
                    isKicking = true;
                }

            } else {

                if(isKicking) {
                    kickTimer = TickTimer.CreateFromSeconds(Runner, kickTimeout);
                    isKicking = false;
                }

                if(isCarrying && chargePercentage > 0) {
                    kickTimer = TickTimer.CreateFromSeconds(Runner, kickTimeout);
                    Shoot();
                }
            }
            if (networkInputData.isSecondaryPressed) {

                if(isCarrying && !isSucking) {
                    suckTimer = TickTimer.CreateFromSeconds(Runner, suckTimeout);
                    Pass();
                }

                if(!isCarrying && suckTimer.Expired(Runner)) {
                    suckTimer = TickTimer.None;
                    attractTimer = TickTimer.CreateFromSeconds(Runner, attractWaitTime);
                    Suck();
                    isSucking = true;
                }

                if(!isCarrying && attractTimer.Expired(Runner)) {
                    Attract();
                }

            } else {

                if(isSucking) {
                    suckTimer = TickTimer.CreateFromSeconds(Runner, suckTimeout);
                    attractTimer = TickTimer.None;
                    isSucking = false;
                }

            }
        }

    }


    public void Update() {
        if(isCarrying && !ballModel.gameObject.activeSelf) {
            LocalAttach();
        }
        if(!isCarrying && ballModel.gameObject.activeSelf){
            LocalDetach();
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
            chargePercentage = 0;
            if(isLocalPlayer) {
                GameState.Dispatch(GameState.SetChargePercentage, chargePercentage, () => {});
            }
        }

    }
    public void Shoot() {
        if(isCarrying) {
            Shoot(maxShootingSpeed * (chargePercentage/100f));
        }
    }

    public void Pass() {
        if(isCarrying) {
            Shoot(passSpeed);
        }
    }

    public void Attract() {
        if(!ball.isAttached) {
            RaycastHit[] hits = Physics.SphereCastAll(transform.position + transform.forward.normalized * attractOffset, attractRadius, transform.forward, attractDistance);
            foreach (var hit in hits) {
                BallController b = hit.transform.GetComponent<BallController>();
                if(b) {
                    Vector3 attraction = transform.forward * -1f * attractStrength * Runner.DeltaTime;
                    b.ApplyForce(attraction);
                }
            }
        }
    }


    public void Suck() {
        if(Object.HasStateAuthority) {
            if(!ball.isAttached) {
                RaycastHit[] hits = Physics.SphereCastAll(transform.position + transform.forward.normalized * suckOffset, suckRadius, transform.forward, suckDistance);
                foreach (var hit in hits) {
                    BallController b = hit.transform.GetComponent<BallController>();
                    if(b) {
                        isCarrying = true;
                        b.Attach(ballAnchor);

                        LocalAttach();
                    }
                }
            }
        }
    }

    public void Kick() {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position + transform.forward.normalized * kickOffset, kickRadius, transform.forward, kickDistance);
        if(!ball.isAttached) {
            foreach (var hit in hits) {
                BallController b = hit.transform.GetComponent<BallController>();
                if(b) {
                    Shoot(kickBallSpeed);
                }
            }
        } else {
            foreach (var hit in hits) {
                PlayerController player = hit.transform.GetComponent<PlayerController>();
                if(player && player.ballGunController.isCarrying) {
                    if(Object.HasStateAuthority) {
                        player.ballGunController.isCarrying = false;
                    }
                    player.ballGunController.LocalDetach();
                    
                    Shoot(kickBallDropSpeed);
                }
            }
        }

    }

    private void Shoot(float inputSpeed) {
        if(Object.HasStateAuthority) {
            isCarrying = false;
        }

        Vector3 forward =
            (transform.forward.normalized * inputSpeed) +
            (transform.forward.normalized * ball.GetComponent<Rigidbody>().velocity.magnitude) +
            playerController.GetComponent<CharacterController>().velocity;
        ball.Shoot(forward);
        Decharge();
    }


    private void LocalAttach() {
        ShowBallExternally();
        if(isLocalPlayer) {
            ShowBallLocally();
        }
    }

    private void LocalDetach() {
        HideBallExternally();
        if(isLocalPlayer) {
            HideBallLocally();
        }
    }

    private void ShowBallLocally() {
        localBallCamera.enabled = true;
        GameState.Dispatch(GameState.SetCarryingBall, true, () => {});
    }
    private void HideBallLocally() {
        localBallCamera.enabled = false;
        GameState.Dispatch(GameState.SetCarryingBall, false, () => {});
    }
    private void ShowBallExternally() {
        ballModel.gameObject.SetActive(true);
    }
    private void HideBallExternally() {
        ballModel.gameObject.SetActive(false);
    }

}