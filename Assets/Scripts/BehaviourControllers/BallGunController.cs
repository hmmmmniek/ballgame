using System;
using Fusion;
using UnityEngine;


public class BallGunController : NetworkBehaviour {
    [Header("Ball gun Settings")]
    public float maxShootingSpeed = 50;
    public float passSpeed = 30;
    public float chargeTimeAmount = 2;

    public float suckDistance = 5;
    public float suckAngle = 20f;
    public float suckTimeout = 0.5f;

    public float attractDistance = 6;
    public float attractAngle = 30f;
    public float attractWaitTime = 0.5f;
    public float attractStrength = 20f;

    public float kickDistance = 7f;
    public float kickAngle = 40f;
    public float kickTimeout = 0.5f;
    public float kickBallDropSpeed = 4;
    public float kickBallSpeed = 20;
    public float kickPushForce = 5;

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
    public bool localShoot { get { return _localShoot; } set { _localShoot = value; } }
    private bool _localKick;
    public bool localKick { get { return _localKick; } set { _localKick = value; } } 
    private bool _localPass;
    public bool localPass { get { return _localPass; } set { _localPass = value; } } 
    private bool _localSuck;
    public bool localSuck { get { return _localSuck; } set { _localSuck = value; } } 



    [HideInInspector][Networked(OnChanged = nameof(OnCarryingChanged))] public bool isCarrying { get; set; }
    public static void OnCarryingChanged(Changed<BallGunController> changed) {
        changed.Behaviour.OnCarryingChanged();
    }
    private void OnCarryingChanged() {
        localShoot = false;
        localPass = false;
        localChargeTime = -1;

        if(isCarrying && !ballModel.gameObject.activeSelf) {
            LocalAttach();
        }
        if(!isCarrying && ballModel.gameObject.activeSelf){
            LocalDetach();
        }
    }

    [HideInInspector][Networked(OnChanged = nameof(OnKickReceived))] public bool kickReceived { get; set; }
    public static void OnKickReceived(Changed<BallGunController> changed) {
        changed.Behaviour.OnKickReceived();
    }
    private void OnKickReceived() {
        if(kickReceived && Object.HasInputAuthority) {
            localKick = false;
        }
    }

    [HideInInspector][Networked(OnChanged = nameof(OnSuckReceived))] public bool suckReceived { get; set; }
    public static void OnSuckReceived(Changed<BallGunController> changed) {
        changed.Behaviour.OnSuckReceived();
    }
    private void OnSuckReceived() {
        if(suckReceived && Object.HasInputAuthority && !InputHandler.instance.localInputDataCache.secondaryPressed) {
            localSuck = false;
        }
    }


    private float clientChargeTime;
    private bool clientShoot;
    private bool clientKick;
    private bool clientPass;
    private bool clientSuck;


    private Action unsubscribePlayers;
    private (BallGunController ballGunController, PlayerController playerController)[] players;



    protected void Awake() {
    }

 
    public override void Spawned() {
        if (!Object.HasInputAuthority) {
            localBallCamera.gameObject.SetActive(false);
        } else {
            GameState.Dispatch<float>(GameState.SetChargeTime, chargeTimeAmount, () => {});
        }
        localChargeTime = -1;
        isCarrying = false; 
        suckTimer = TickTimer.CreateFromSeconds(Runner, 0);
        kickTimer = TickTimer.CreateFromSeconds(Runner, 0);
        LocalDetach();


        unsubscribePlayers = GameState.Select<(BallGunController ballGunController, PlayerController playerController)[]>(GameState.GetPlayers, (players) => {
            if (players != null) {
                this.players = players;
            }
        });
    }

    private bool previousPrimaryPressed = false;
    public void Update() {
        if(Object.HasInputAuthority) {
            /*
            * Start kick
            */
            if (
                InputHandler.instance.localInputDataCache.primaryPressed &&
                !isCarrying &&
                kickTimer.Expired(Runner)
            ) {   
                localKick = true;
            }

            /*
            * Stop kick
            */
            if (
                !InputHandler.instance.localInputDataCache.primaryPressed &&
                localKick &&
                kickReceived
            ) {   
                localKick = false;
            }


            /*
            * Start pass
            */
            if (
                InputHandler.instance.localInputDataCache.secondaryPressed &&
                isCarrying &&
                !localSuck
            ) {   
                localPass = true;
            }

            /*
            * Start suck
            */
            if (
                InputHandler.instance.localInputDataCache.secondaryPressed &&
                !isCarrying &&
                suckTimer.Expired(Runner) &&
                !localSuck
            ) {   
                localSuck = true;
            }

            /*
            * Stop suck
            */
            if (
                !InputHandler.instance.localInputDataCache.secondaryPressed &&
                localSuck &&
                suckReceived
            ) {   
                localSuck = false;
            }


            /*
            * Charge
            */
            if (
                InputHandler.instance.localInputDataCache.primaryPressed &&
                localChargeTime == -1 && 
                isCarrying
            ) {   
                localChargeTime = Runner.SimulationTime;
            }

            /*
            * Shoot charged shot
            */
            if(
                previousPrimaryPressed == true &&
                !InputHandler.instance.localInputDataCache.primaryPressed &&
                isCarrying &&
                localChargeTime != -1
            ) {
                localShoot = true;
            }

            previousPrimaryPressed = InputHandler.instance.localInputDataCache.primaryPressed;
            
        }

    }

    private float previousClientChargeTime = -1;
    private bool previousClientSuck = false;
    public override void FixedUpdateNetwork() {

        if(Object.HasStateAuthority) {

            if (GetInput(out NetworkInputData networkInputData)) {
                clientChargeTime = networkInputData.clientChargeTime;
                clientShoot = networkInputData.clientShoot;
                clientKick = networkInputData.clientKick;
                clientPass = networkInputData.clientPass;
                clientSuck = networkInputData.clientSuck;
            }

            /*
            * Stop kick
            */
            if (
                !clientKick
            ) {   
                kickReceived = false;
            }

            /*
            * Execute kick
            */
            if (
                clientKick &&
                !isCarrying &&
                kickTimer.Expired(Runner)
            ) {   
                kickTimer = TickTimer.CreateFromSeconds(Runner, kickTimeout);
                Kick();
                kickReceived = true;
            }

            /*
            * Execute pass
            */
            if (
                clientPass &&
                isCarrying
            ) {   
                suckTimer = TickTimer.CreateFromSeconds(Runner, suckTimeout);
                Pass();
            }


            /*
            * Execute suck
            */
            if(
                clientSuck &&
                !isCarrying &&
                suckTimer.Expired(Runner)
            ) {
                suckTimer = TickTimer.None;
                attractTimer = TickTimer.CreateFromSeconds(Runner, attractWaitTime);
                Suck();
                suckReceived = true;
            }

            /*
            * Attract
            */
            if(
                clientSuck &&
                !isCarrying &&
                attractTimer.Expired(Runner)
            ) {
                Attract();
            }
            
            /*
            * Reset sucking
            */
            if(
                previousClientSuck == true &&
                !clientSuck
            ) {
                suckTimer = TickTimer.CreateFromSeconds(Runner, suckTimeout);
                attractTimer = TickTimer.None;
                suckReceived = false;
            }


            /*
            * Execute charged shot
            */
            if(
                clientChargeTime != -1 &&
                previousClientChargeTime == -1 &&
                Runner.SimulationTime - clientChargeTime > 0.5f // Dont allow sudden large change in charge time
            ) {
                clientChargeTime = -1;
            }

            if(
                clientShoot &&
                isCarrying
            ) {
                kickTimer = TickTimer.CreateFromSeconds(Runner, kickTimeout);
                ShootCharged(Math.Clamp((Runner.SimulationTime - clientChargeTime) / chargeTimeAmount * 100, 0, 100));
            }
            previousClientSuck = clientSuck;
            previousClientChargeTime = clientChargeTime;
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
            Vector3 playerToBall = (ball.transform.position - transform.position);
            float distance = playerToBall.magnitude;

            if(distance > attractDistance) {
                return;
            }
            
            float angle = Vector3.Angle(playerToBall.normalized, transform.forward);
            if(angle > attractAngle) {
                return;
            }
            Vector3 attraction = transform.forward * -1f * attractStrength * Runner.DeltaTime;
            ball.ApplyForce(attraction);
        }
    }


    public void Suck() {
        if(!ball.isAttached) {
            Vector3 playerToBall = (ball.transform.position - transform.position);
            float distance = playerToBall.magnitude;

            if(distance > suckDistance) {
                return;
            }
            
            float angle = Vector3.Angle(playerToBall.normalized, transform.forward);

            if(angle > suckAngle) {
                return;
            }

            isCarrying = true;
            ball.Attach(ballAnchor);
        }
    }

    public void Kick() {
        bool justDroppedBall = false;
        foreach (var player in players) {
            if(player.ballGunController == this) {
                continue;
            }
            
            Vector3 playerToPlayer = (player.playerController.transform.position - transform.position);
            float distance = playerToPlayer.magnitude;

            if(distance > kickDistance) {
                continue;
            }
            
            float angle = Vector3.Angle(playerToPlayer.normalized, transform.forward);

            if(angle > kickAngle) {
                continue;
            }
            if(player.ballGunController.isCarrying) {
                if(Object.HasStateAuthority) {
                    player.ballGunController.isCarrying = false;
                }
                Shoot(kickBallDropSpeed);
                justDroppedBall = true;
            }
            player.playerController.GetComponent<CharacterMovementController>().Push(transform.forward.normalized * kickPushForce);
        }
            
        if(!ball.isAttached && !justDroppedBall) {
            Vector3 playerToBall = (ball.transform.position - transform.position);
            float distance = playerToBall.magnitude;

            if(distance > kickDistance) {
                return;
            }
            
            float angle = Vector3.Angle(playerToBall.normalized, transform.forward);
            if(angle > kickAngle) {
                return;
            }

            Shoot(kickBallSpeed);
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

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        unsubscribePlayers();
    }


}