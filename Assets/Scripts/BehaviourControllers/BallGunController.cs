using System;
using Fusion;
using UnityEngine;


public class BallGunController : NetworkBehaviour {
    [Header("Ball gun Settings")]
    public float maxShootingSpeed = 50;
    public float passSpeed = 30;
    public float chargeTimeAmount = 2;

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

    public float maxAllowedClientChargeError = 7;

    public Camera localBallCamera;
    public PlayerController playerController;
    public NetworkTransform ballAnchor;
    public Transform ballModel;

    [HideInInspector]
    public BallController ball;

    
    [Networked] private TickTimer suckTimer { get; set; }
    [Networked] private TickTimer attractTimer { get; set; }
    [Networked] private TickTimer kickTimer { get; set; }

    [Networked] private float networkChargeTime { get; set; }
    private float _localChargeTime;
    public float localChargeTime {
        get {
            return _localChargeTime;
        }
        set {
            _localChargeTime = value;
            if(Object.HasInputAuthority) {
                if(value == -1) {
                    GameState.Dispatch<bool>(GameState.SetIsCharging, false, () => {});
                } else {
                    GameState.Dispatch<bool>(GameState.SetIsCharging, true, () => {});
                }
            }
        }
    }
    private bool _localShoot;
    public bool localShoot { get { return _localShoot; } set { _localShoot = value; } } // No idea why, but without the getter/setter it doesnt work??
    
    [HideInInspector][Networked(OnChanged = nameof(OnCarryingChanged))] public bool isCarrying { get; set; }
    public static void OnCarryingChanged(Changed<BallGunController> changed) {
        changed.Behaviour.OnCarryingChanged();
    }
    private void OnCarryingChanged() {
        localShoot = false;
        localChargeTime = -1;
        
        if(Object.HasStateAuthority) {
            networkChargeTime = -1;
        }

        if(isCarrying && !ballModel.gameObject.activeSelf) {
            LocalAttach();
        }
        if(!isCarrying && ballModel.gameObject.activeSelf){
            LocalDetach();
        }
    }

    private bool isSucking;
    private bool isKicking;

    private float inputSendTime;
    private float primaryPressedTime;
    private float primaryReleaseTime;
    private float secondaryPressedTime;
    private float secondaryReleaseTime;
    private float clientChargeTime;
    private bool clientShoot;


    protected void Awake() {
    }

 
    public override void Spawned() {
        if (!Object.HasInputAuthority) {
            localBallCamera.gameObject.SetActive(false);
        } else {
            GameState.Dispatch<float>(GameState.SetChargeTime, chargeTimeAmount, () => {});
        }
        networkChargeTime = -1;
        isCarrying = false; 
        suckTimer = TickTimer.CreateFromSeconds(Runner, 0);
        kickTimer = TickTimer.CreateFromSeconds(Runner, 0);
        LocalDetach();
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);
    }

    private bool a = false;
    private float t = 0;
    public override void FixedUpdateNetwork() {
  
       // if(Object.HasStateAuthority) {
            bool receivedInput = false;

            if (GetInput(out NetworkInputData networkInputData)) {
                primaryPressedTime = networkInputData.primaryPressedTime;
                primaryReleaseTime = networkInputData.primaryReleaseTime;
                secondaryPressedTime = networkInputData.secondaryPressedTime;
                secondaryReleaseTime = networkInputData.secondaryReleaseTime;
                clientChargeTime = networkInputData.clientChargeTime;
                clientShoot = networkInputData.clientShoot;
                inputSendTime = networkInputData.runnerTime;
                receivedInput = true;
            }

            /*
            * Start kick
            */
        // if (
        //     primaryPressedTime == inputSendTime &&
        //     !isCarrying &&
        //     kickTimer.Expired(Runner)
        // ) {   
        //     kickTimer = TickTimer.None;
        //     Kick();
        //     isKicking = true;
        // }

            /*
            * Charge
            */
            if (
                primaryPressedTime == inputSendTime &&
                localChargeTime == -1 && 
                networkChargeTime == -1 &&
                isCarrying
            ) {   
                if(Object.HasStateAuthority) {
                    networkChargeTime = Runner.SimulationTime;
                    localChargeTime = networkChargeTime;
                } else if(Object.HasInputAuthority) {
                    localChargeTime = Runner.SimulationTime;
                }
            }
    
            /*
            * Stop kick
            */
        // if(
        //     primaryReleaseTime == inputSendTime &&
        //     isKicking
        // ) {
        //     kickTimer = TickTimer.CreateFromSeconds(Runner, kickTimeout);
        //     isKicking = false;
        // }
            
            /*
            * Shoot charged shot
            */
            if(
                receivedInput &&
                primaryReleaseTime == inputSendTime &&
                isCarrying &&
                localChargeTime != -1
            ) {
                localShoot = true;
            }

            /*
            * Pass
            */
        // if(
        //     secondaryPressedTime == inputSendTime &&
        //     isCarrying &&
        //     !isSucking
        // ) {
        //     suckTimer = TickTimer.CreateFromSeconds(Runner, suckTimeout);
        //     Pass();
        // }

            /*
            * Suck
            */
        // if(
        //     secondaryPressedTime == inputSendTime &&
        //     !isCarrying &&
        //     suckTimer.Expired(Runner)
        // ) {
        //     suckTimer = TickTimer.None;
        //     attractTimer = TickTimer.CreateFromSeconds(Runner, attractWaitTime);
        //     Suck();
        //     isSucking = true;
        // }

            /*
            * Attract
            */
        // if(
        //     secondaryPressedTime == inputSendTime &&
        //     !isCarrying &&
        //     attractTimer.Expired(Runner)
        // ) {
        //     Attract();
        // }

            /*
            * Reset sucking
            */
        // if(
        //     secondaryReleaseTime == inputSendTime &&
        //     isSucking
        // ) {
        //     suckTimer = TickTimer.CreateFromSeconds(Runner, suckTimeout);
        //     attractTimer = TickTimer.None;
        //     isSucking = false;
        // }

        // }
          
        if( 
            Object.HasInputAuthority &&
            (networkChargeTime != -1 && localChargeTime != -1 && Math.Abs(localChargeTime - networkChargeTime) > maxAllowedClientChargeError)
        ) {
            localChargeTime = networkChargeTime;
        }

        if(
            Object.HasStateAuthority &&
            receivedInput && 
            (Math.Abs(networkChargeTime - clientChargeTime) < maxAllowedClientChargeError || (networkChargeTime == -1 && Math.Abs(Runner.SimulationTime - clientChargeTime) < maxAllowedClientChargeError)) 
        ) {
            networkChargeTime = clientChargeTime;
        }

        if(Object.HasStateAuthority && (clientShoot || localShoot)) {
            kickTimer = TickTimer.CreateFromSeconds(Runner, kickTimeout);
            ShootCharged(Math.Clamp((Runner.SimulationTime - networkChargeTime) / chargeTimeAmount * 100, 0, 100));
        }
    }

    public void ShootCharged(float charge) {
        if(isCarrying) {
            Shoot(maxShootingSpeed * (charge/100f));
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
    }


    private void LocalAttach() {
        ShowBallExternally();
        if(Object.HasInputAuthority) {
            ShowBallLocally();
        }
    }

    private void LocalDetach() {
        HideBallExternally();
        if(Object.HasInputAuthority) {
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