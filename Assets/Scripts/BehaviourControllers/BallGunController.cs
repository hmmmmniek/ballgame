using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class BallGunController : NetworkBehaviour {
    [Header("Ball gun Settings")]
    public float maxShootingSpeed = 50;
    public float passSpeed = 30;
    public float chargeTimeAmount = 2;
    public float maxShootRecoilForce = 10;

    public float suckDistance = 5;
    public float suckAngle = 20f;
    public float suckTimeout = 0.5f;
    public float suckPushForce = 5f;

    public float attractDistance = 6;
    public float attractAngle = 30f;
    public float attractWaitTime = 0.5f;
    public float attractStrength = 20f;

    public float kickDistance = 7f;
    public float kickAngle = 40f;
    public float kickTimeout = 0.5f;
    public float kickBallDropSpeed = 4;
    public float kickBallDropDistance = 3f;
    public float kickBallSpeed = 20;
    public float kickPushForce = 5;

    public float shieldDistance = 2;
    public float shieldThickness = 2;
    public float shieldWidth = 5;
    public float shieldAngle = 90;
    public float shieldPushBack = 2f;
    public float shieldMinGrabSpeed = 2f;
    public float shieldHeightPosition = 0.5f;
    
    public float spinPullDistance = 1f;

    public float maxAllowedClientChargeError = 7;


    public Camera localBallCamera;
    public PlayerController playerController;
    public NetworkTransform ballAnchor;
    public Transform ballModel;
    [HideInInspector]
    public BallController ball;
    public VisualEffect[] ballShootEffects;
    public VisualEffect[] kickEffects;
    public VisualEffect suckEffect;
    public VisualEffect suckQuickEffect;
    public VisualEffect shieldEffect;


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
    private bool _localShield;
    public bool localShield {
        get {
            return _localShield;
        }
        set {
            if(value != _localShield) {
                GameState.Dispatch<bool>(GameState.SetShielding, value, () => {});
                _localShield = value;
            }
            
        }
    } 
    private bool _localBallSpin;
    public bool localBallSpin {
        get {
            return _localBallSpin;
        }
        set {
            if(value != _localBallSpin) {
                GameState.Dispatch<bool>(GameState.SetInputtingSpin, value, () => {});
                _localBallSpin = value;
            }
            
        }
    } 
    private bool _localBallRoll;
    public bool localBallRoll {
        get {
            return _localBallRoll;
        }
        set {
            if(value != _localBallRoll) {
                GameState.Dispatch<bool>(GameState.SetInputtingRoll, value, () => {});
                _localBallRoll = value;
            }
        }
    } 
    private Vector2 _localSpinRotationInputStart;
    public Vector2 localSpinRotationInputStart { get { return _localSpinRotationInputStart; } set { _localSpinRotationInputStart = value; } } 



    [HideInInspector][Networked(OnChanged = nameof(OnCarryingChanged))] public bool isCarrying { get; set; }
    public static void OnCarryingChanged(Changed<BallGunController> changed) {
        changed.Behaviour.OnCarryingChanged();
    }
    private void OnCarryingChanged() {
        localShoot = false;
        localPass = false;
        attracting = false;
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
        if(kickReceived && Object.HasInputAuthority && !InputHandler.instance.localInputDataCache.primaryPressed) {
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

    [HideInInspector][Networked(OnChanged = nameof(OnAttractingChanged))] public bool attracting { get; set; }
    public static void OnAttractingChanged(Changed<BallGunController> changed) {
        changed.Behaviour.OnAttractingChanged();
    }
    private void OnAttractingChanged() {
        if(Object.HasInputAuthority) {
            GameState.Dispatch<bool>(GameState.SetAttracting, attracting, () => {});
        }
        if(attracting) {
            suckEffect.SendEvent("StartContinuous");
        } else {
            suckEffect.SendEvent("StopContinuous");
        }
    }

    [HideInInspector][Networked(OnChanged = nameof(OnShieldingChanged))] public bool shielding { get; set; }
    public static void OnShieldingChanged(Changed<BallGunController> changed) {
        changed.Behaviour.OnShieldingChanged();
    }
    private void OnShieldingChanged() {
        if(shielding) {
            shieldEffect.SendEvent("StartContinuous");
        } else {
            shieldEffect.SendEvent("StopContinuous");
        }
    }

    private float clientChargeTime;
    private bool clientShoot;
    private bool clientKick;
    private bool clientPass;
    private bool clientSuck;
    private bool clientShield;
    private bool clientBallRoll;
    private bool clientBallSpin;
    private Vector2 clientBallSpinRotationStart;
    private Vector2 clientBallSpinRotationEnd;
    private Action unsubscribePlayers;
    private Player[] players;
    private bool spawned = false;

    protected void Awake() {

    }

 
    public override void Spawned() {
        

        if (Object.HasInputAuthority) {
            GameState.Dispatch<float>(GameState.SetChargeTime, chargeTimeAmount, () => {});
        }
        localChargeTime = -1;
        isCarrying = false; 
        suckTimer = TickTimer.CreateFromSeconds(Runner, 0);
        kickTimer = TickTimer.CreateFromSeconds(Runner, 0);
        LocalDetach();


        unsubscribePlayers = GameState.Select<Player[]>(GameState.GetPlayers, (players) => {
            if (players != null) {
                this.players = players.Where((p) => p.playerController != null && p.team.HasValue).ToArray();
            }
        });

        spawned = true;
    }

    private bool previousPrimaryPressed = false;
    public void Update() {
        if(spawned && Object.HasInputAuthority) {

            /*
            * Pull ball spin
            */
            if (
                InputHandler.instance.localInputDataCache.ballSpinPressed || InputHandler.instance.localInputDataCache.ballRollPressed
            ) {
                localBallSpin = InputHandler.instance.localInputDataCache.ballSpinPressed;
                localBallRoll = InputHandler.instance.localInputDataCache.ballSpinPressed ? false : InputHandler.instance.localInputDataCache.ballRollPressed;
                Vector2 spinPullCurrentToStart = (localSpinRotationInputStart - InputHandler.instance.networkInputDataCache.rotationInput);
                float magnitude = 0;
                if(localBallSpin) {
                    magnitude = spinPullCurrentToStart.magnitude;
                }
                if(localBallRoll) {
                    magnitude = Math.Abs(localSpinRotationInputStart.x - InputHandler.instance.networkInputDataCache.rotationInput.x);
                }
                
                if(magnitude > spinPullDistance) {
                    localSpinRotationInputStart = InputHandler.instance.networkInputDataCache.rotationInput + spinPullCurrentToStart.normalized * spinPullDistance;
                }
                
                if(localBallSpin) {
                    spinPullCurrentToStart = (localSpinRotationInputStart - InputHandler.instance.networkInputDataCache.rotationInput);
                    GameState.Dispatch<Vector2>(GameState.SetSpinInput,  spinPullCurrentToStart.normalized * (spinPullCurrentToStart.magnitude / spinPullDistance), () => {});
                }
                if(localBallRoll) {
                    GameState.Dispatch<float>(GameState.SetRollInput, (localSpinRotationInputStart.x - InputHandler.instance.networkInputDataCache.rotationInput.x) / spinPullDistance, () => {});
                }
                

            }

            /*
            * Reset ball spin
            */
            if (
                !InputHandler.instance.localInputDataCache.ballSpinPressed && !InputHandler.instance.localInputDataCache.ballRollPressed
            ) {
                localBallSpin = false;
                localBallRoll = false;
                localSpinRotationInputStart = InputHandler.instance.networkInputDataCache.rotationInput;
            }
            if (
                !InputHandler.instance.localInputDataCache.ballSpinPressed
            ) {
                localBallSpin = false;
            }
            if (
                !InputHandler.instance.localInputDataCache.ballRollPressed
            ) {
                localBallRoll = false;
            }
            // I love you, how did i get so lucky??
            // you're the hottest, smartest, sweetest, funniest, sexiest game developer I've ever met
            // will you be my boyfriend? <3
            /*
            * Start kick
            */
            if (
                InputHandler.instance.localInputDataCache.primaryPressed &&
                !isCarrying &&
                kickTimer.Expired(Runner) &&
                !localKick
            ) {   
                localKick = true;
                kickEffects[9].SendEvent("Burst");
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
                ballShootEffects[3].SendEvent("Burst");
                kickEffects[3].SendEvent("Burst");
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
                suckQuickEffect.SendEvent("Burst");
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
            * Start shield
            */
            if (
                InputHandler.instance.localInputDataCache.shieldPressed &&
                localShield == false && 
                !isCarrying
            ) {   
                localShield = true;
                shieldEffect.SendEvent("StartContinuous");
            }

            /*
            * Stop shield
            */
            if (
                (
                    !InputHandler.instance.localInputDataCache.shieldPressed &&
                    localShield == true
                ) || 
                isCarrying
            ) {   
                localShield = false;
                shieldEffect.SendEvent("StopContinuous");
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
                float charge = Math.Clamp((Runner.SimulationTime - localChargeTime) / chargeTimeAmount, 0, 1);
                ballShootEffects[(int)Math.Ceiling(charge * 10) - 1].SendEvent("Burst");
                kickEffects[(int)Math.Ceiling(charge * 10) - 1].SendEvent("Burst");
            }

            previousPrimaryPressed = InputHandler.instance.localInputDataCache.primaryPressed;
            
        }

    }

    private float previousClientChargeTime = -1;
    private bool previousClientSuck = false;
    private bool previousClientKick = false;
    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {
            clientChargeTime = networkInputData.clientChargeTime;
            clientShoot = networkInputData.clientShoot;
            clientKick = networkInputData.clientKick;
            clientPass = networkInputData.clientPass;
            clientSuck = networkInputData.clientSuck;
            clientShield = networkInputData.clientShield;
            clientBallRoll = networkInputData.clientBallRoll;
            clientBallSpin = networkInputData.clientBallSpin;
            clientBallSpinRotationStart = networkInputData.clientBallSpinRotationStart;
            clientBallSpinRotationEnd = networkInputData.rotationInput;
        }

        if(Object.HasStateAuthority) {

            /*
            * Execute pass
            */
            if (
                clientPass &&
                isCarrying
            ) {
                suckTimer = TickTimer.CreateFromSeconds(Runner, suckTimeout);
                Pass();
                RPC_ExecuteEffect(1, 0);
            }

            /*
            * Execute kick
            */
            if (
                clientKick &&
                !isCarrying &&
                kickTimer.Expired(Runner)
            ) {   
                kickTimer = TickTimer.None;
                Kick();
                kickReceived = true;
                RPC_ExecuteEffect(2, 0);
            }

            /*
            * Reset kicking
            */
            if (
                previousClientKick == true &&
                !clientKick
            ) {   
                kickTimer = TickTimer.CreateFromSeconds(Runner, kickTimeout);
                kickReceived = false;
            }

            /*
            * Shield
            */
            if(
                clientShield &&
                !isCarrying
            ) {
                if(!shielding) {
                    shielding = true;
                }
                Reflect();
            } else {
                if(shielding) {
                    shielding = false;
                }
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
                RPC_ExecuteEffect(3, 0);

            }

            /*
            * Reset sucking
            */
            if(
                previousClientSuck == true &&
                !clientSuck
            ) {
                suckTimer = TickTimer.CreateFromSeconds(Runner, suckTimeout);
                suckReceived = false;
                attracting = false;
            }

            /*
            * Attract
            */
            if(
                clientSuck &&
                !isCarrying &&
                attractTimer.Expired(Runner)
            ) {
                attracting = true;
                attractTimer = TickTimer.None;
            }
            if(attracting) {
                Attract();
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
                float charge = Math.Clamp((Runner.SimulationTime - clientChargeTime) / chargeTimeAmount, 0, 1);
                ShootCharged(charge * 100);
                RPC_ExecuteEffect(0, charge);
            }

        }



                    

        previousClientSuck = clientSuck;
        previousClientChargeTime = clientChargeTime;
        previousClientKick = clientKick;
    }



    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, InvokeLocal = true, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_ExecuteEffect(int effectType, float charge, RpcInfo info = default){
        if(!Object.HasInputAuthority) {
            switch(effectType) {
                case 0: {
                    ballShootEffects[(int)Math.Ceiling(charge * 10) - 1].SendEvent("Burst");
                    kickEffects[(int)Math.Ceiling(charge * 10) - 1].SendEvent("Burst");
                    break;
                }
                case 1: {
                    ballShootEffects[3].SendEvent("Burst");
                    kickEffects[3].SendEvent("Burst");
                    break;
                }
                case 2: {
                    kickEffects[9].SendEvent("Burst");
                    break;
                }
                case 3: {
                    suckQuickEffect.SendEvent("Burst");
                    break;
                }
                default: {
                    break;
                }
            }
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
        if(!ball.isAttached && MatchController.instance.state == State.Started) {
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

        foreach (var player in players) {
            if(player.ballGunController == this) {
                continue;
            }
            
            Vector3 playerToPlayer = (player.playerController.transform.position - transform.position);
            float distance = playerToPlayer.magnitude;

            if(distance > suckDistance) {
                continue;
            }
            
            float angle = Vector3.Angle(playerToPlayer.normalized, transform.forward);

            if(angle > suckAngle) {
                continue;
            }
         
            player.playerController.GetComponent<CharacterMovementController>().Push((-transform.forward.normalized) * suckPushForce);
            playerController.localCharacterMovementController.networkMovementController.Push(transform.forward.normalized * suckPushForce);

        }

        if(!ball.isAttached && MatchController.instance.state == State.Started) {
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
            if(player.ballGunController.isCarrying && distance <= kickBallDropDistance && playerController.team != player.team) {
                if(Object.HasStateAuthority) {
                    player.ballGunController.isCarrying = false;
                }
                Shoot(kickBallDropSpeed);
                justDroppedBall = true;
            }
            CharacterMovementController otherCharacterController = player.playerController.GetComponent<CharacterMovementController>();
            float againstFraction = Math.Clamp(-Vector3.Dot(transform.forward, otherCharacterController.Velocity.normalized), 0, 1);
            Vector3 pushDirection = (transform.forward + otherCharacterController.transform.up).normalized;
            otherCharacterController.Push((pushDirection * kickPushForce) + (-otherCharacterController.Velocity * againstFraction));
            playerController.localCharacterMovementController.networkMovementController.Push(-(transform.forward.normalized) * kickPushForce);

        }
            
        if(!ball.isAttached && !justDroppedBall && MatchController.instance.state == State.Started) {
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

    public void Shoot(float inputSpeed) {
        if(Object.HasStateAuthority) {
            isCarrying = false;
        }

        Vector3 forward = 
            (transform.forward.normalized * inputSpeed) +
            (transform.forward.normalized * ball.GetComponent<Rigidbody>().velocity.magnitude) +
            playerController.GetComponent<CharacterController>().velocity
        ;
        ball.Shoot(forward, GetSpinInput(), GetRollInput());

        Vector3 recoil = (-transform.forward.normalized) * maxShootRecoilForce * (forward.magnitude / ball.maxSpeed);
        playerController.localCharacterMovementController.networkMovementController.Push(recoil);

  
    }

    private void Reflect() {
        if(!ball.isAttached && MatchController.instance.state == State.Started) {
            Vector3 shieldCenter = (transform.position + -transform.up * shieldHeightPosition + transform.forward * shieldDistance);

            Vector3 shieldToBall = (ball.transform.position - shieldCenter);
            float distance = shieldToBall.magnitude;

            float angle = Vector3.Angle(shieldToBall.normalized, transform.forward);
            if(angle > shieldAngle) {
                return;
            }

            float a = shieldThickness;
            float b = a * (float)Math.Tan(angle * (Math.PI/180));
            float shieldDistanceOnAngle = Math.Clamp((float)Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2)), shieldThickness, shieldWidth);

            if(distance > shieldDistanceOnAngle) {
                return;
            }

            Vector3 pos = shieldCenter + shieldToBall.normalized * shieldDistanceOnAngle;
            ball.transform.position = pos;

            Vector3 shieldDirection = transform.forward;
            Vector3 playerVelocity = playerController.GetComponent<CharacterController>().velocity;
            Vector3 bounceDir = shieldDirection.normalized;
                
            float ballVelocity = ball.getVelocity();
            if(ballVelocity < shieldMinGrabSpeed) {
                ball.Shoot(bounceDir * Vector3.Dot(shieldDirection.normalized, playerVelocity.normalized) * playerVelocity.magnitude * shieldPushBack, new Vector2(), 0);
            } else {
                ball.Shoot(bounceDir * ballVelocity, GetSpinInput(), GetRollInput());
            }
        }
    }


    private Vector2 GetSpinInput() {
        if(!clientBallSpin) {
            return new Vector2();
        }
        Vector2 pullDirection = clientBallSpinRotationEnd - clientBallSpinRotationStart;
        Vector2 spinInput = pullDirection.normalized * (pullDirection.magnitude / spinPullDistance);
        return spinInput;
    }

    private float GetRollInput() {
        if(!clientBallRoll) {
            return 0;
        }
        float pullDirection = clientBallSpinRotationEnd.x - clientBallSpinRotationStart.x;
        float rollInput = pullDirection / spinPullDistance;
        return rollInput;
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