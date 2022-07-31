using System;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.VFX;

[OrderBefore(typeof(NetworkTransform))]
[DisallowMultipleComponent]
// ReSharper disable once CheckNamespace
public class CharacterMovementController : NetworkBehaviour {
    [Header("Character movement Settings")]
    public float gravity = -20.0f;
    public float jumpImpulse = 5.0f;
    public float boostImpulse = 25.0f;
    public float acceleration = 10.0f;
    public float braking = 10.0f;
    public float maxGroundSpeed = 2.0f;
    public float maxSprintGroundSpeed = 3.5f;
    public float maxBallWalkingSpeed = 1.5f;
    public float maxVerticalSpeed = 6f;
    public float boostUsageSpeed = 40f;
    public float boostRechargeSpeed = 15f;
    public float rememberInputTime = 0.5f;
    public float maxAllowedClientPositionError = 1f;
    public float maxAllowedClientVelocityError = 1f;
    public float maxAllowedClientBoostError = 1f;
    public float dashTimeout = 0.2f;
    public float dashImpulse = 10f;
    public float dashBoostUsage = 50f;
    public float bashPlayerMaximumDistance = 2f;
    public float bashPlayerMinimumSpeed = 30f;
    public float bashPlayerMaximumAngle = 90f;
    public float bashPlayerPushStrength = 10f;
    public float bashPlayerBallDropSpeed = 4f;
    public float bashPlayerKnockoutSpeed = 20f;
    
    public CharacterCameraController cameraController;
    public LocalCharacterMovementController localCharacterMovementController;
    public BallGunController ballGunController;
    public VisualEffect boostEffect;
    public VisualEffect jumpEffect;


    [HideInInspector][Networked]public Vector3 Velocity { get; set; }
    [HideInInspector][Networked(OnChanged = nameof(OnBoostChanged))]
    public float boostRemainingPercentage { get; set; }
    public static void OnBoostChanged(Changed<CharacterMovementController> changed) {
        changed.Behaviour.OnBoostChanged();
    }
    private void OnBoostChanged() {
        if(Object.HasInputAuthority) {
            GameState.Dispatch(GameState.SetRemainingBoostPercentage, boostRemainingPercentage, () => {});
        }
    }


    [HideInInspector][Networked(OnChanged = nameof(OnPushedChanged))]
    public bool pushed { get; set; }
    public static void OnPushedChanged(Changed<CharacterMovementController> changed) {
        changed.Behaviour.OnPushedChanged();
    }
    private void OnPushedChanged() {
        if(Object.HasInputAuthority && pushed) {
            localCharacterMovementController.Velocity = Velocity;
            pushedReceived = true;
        }
        if(Object.HasInputAuthority && !pushed) {
            pushedReceived = false;
        }
    }
    private bool _pushedReceived;
    public bool pushedReceived { get { return _pushedReceived; } set { _pushedReceived = value; } } 
    
    [HideInInspector][Networked(OnChanged = nameof(OnJumpReceived))] public bool jumpReceived { get; set; }
    public static void OnJumpReceived(Changed<CharacterMovementController> changed) {
        changed.Behaviour.OnJumpReceived();
    }
    private void OnJumpReceived() {
        if(jumpReceived && Object.HasInputAuthority && !InputHandler.instance.localInputDataCache.jumpPressed) {
            localCharacterMovementController.localJump = false;
        }
    }

    [HideInInspector][Networked(OnChanged = nameof(OnDashReceived))] public bool dashReceived { get; set; }
    public static void OnDashReceived(Changed<CharacterMovementController> changed) {
        changed.Behaviour.OnDashReceived();
    }
    private void OnDashReceived() {
        if(dashReceived && Object.HasInputAuthority) {
            localCharacterMovementController.localDash = false;
        }
    }

    [HideInInspector][Networked(OnChanged = nameof(OnHitGroundReceived))] public bool hitGroundReceived { get; set; }
    public static void OnHitGroundReceived(Changed<CharacterMovementController> changed) {
        changed.Behaviour.OnHitGroundReceived();
    }
    private void OnHitGroundReceived() {
        if(hitGroundReceived && Object.HasInputAuthority) {
            localCharacterMovementController.localHitGround = false;
        }
    }


    //protected override Vector3 DefaultTeleportInterpolationVelocity => Velocity;
    //protected override Vector3 DefaultTeleportInterpolationAngularVelocity => new Vector3(0f, 0f, InputHandler.instance.lookHorizontalSpeed);
    public CharacterController Controller { get; private set; }

    private Action unsubscribePlayers;
    private Player[] players;



    public void Start() {
    }


    public override void Spawned() {
        base.Spawned();
        Controller = GetComponent<CharacterController>();

        unsubscribePlayers = GameState.Select<Player[]>(GameState.GetPlayers, (players) => {
            if (players != null) {
                this.players = players.Where((p) => p.playerController != null && p.team.HasValue).ToArray();
            }
        });
       
        boostRemainingPercentage = 100f;

        Physics.IgnoreCollision(localCharacterMovementController.GetComponent<CharacterController>(), GetComponent<CharacterController>());
        InitLocalController();
    }

    public async void InitLocalController() {
        await Task.Delay(100);
        localCharacterMovementController.Init(Object.HasInputAuthority);
        localCharacterMovementController.Synchronize();
    }

    private Vector2 movement;
    private bool clientJump;
    private bool clientDash;
    private bool clientHitGround;
    private bool clientSprint;
    private Vector3 clientPosition;
    private Vector3 clientVelocity;
    private float clientBoostRemaining;

    private bool previousClientJump = false;
    private bool previousClientDash = false;
    private bool previousClientHitGround = false;
    private float previousClientBoostRemaining = 0;

    private bool startedBoostEffect = false;
    public override void FixedUpdateNetwork() {

        if(Object.HasStateAuthority) {


            bool receivedInput = false;
            if (GetInput(out NetworkInputData networkInputData)) {
                clientJump = networkInputData.clientJump;
                clientSprint = networkInputData.clientSprint;
                movement = networkInputData.movementInput;
                clientPosition = networkInputData.clientPosition;
                clientVelocity = networkInputData.clientVelocity;
                clientBoostRemaining = networkInputData.clientBoostRemaining;
                clientDash = networkInputData.clientDash;
                clientHitGround = networkInputData.clientHitGround;
                receivedInput = true;
               
                if(networkInputData.pushedReceived) {
                    pushed = false;
                }
                if(pushed) {
                    clientVelocity = Velocity;
                }

            }

            float delta = Runner.DeltaTime;




            
            /*
            * Dash
            */
            if(
                clientDash &&
                !ballGunController.isCarrying &&
                !dashReceived
            ) {
                (float b, Vector3 v) = Utils.Dash(
                    Controller,
                    movement,
                    transform,
                    dashImpulse,
                    dashBoostUsage,
                    boostRemainingPercentage
                );
                boostRemainingPercentage = b;
                if(v.magnitude > 0) {
                    Velocity = v;
                }
                dashReceived = true;
                RPC_ExecuteEffect(2);
            }


            /*
            * Reset dash
            */
            if(
                previousClientDash == true &&
                !clientDash
            ) {
                dashReceived = false;
            }

            /*
            * Jump
            */
            if(
                clientJump
            ) {
                if(Controller.isGrounded) {
                    Velocity = Utils.Jump(
                        delta,
                        Velocity,
                        Controller,
                        jumpImpulse
                    );
                    RPC_ExecuteEffect(3);
                }
                jumpReceived = true;
            }

            /*
            * Reset jump
            */
            if(
                previousClientJump == true &&
                !clientJump
            ) {
                jumpReceived = false;
            }
            /*
            * Boost
            */
            if(
                clientJump &&
                !Controller.isGrounded
            ) {

                if(boostRemainingPercentage > 0 && !startedBoostEffect) {
                    RPC_ExecuteEffect(0);
                    startedBoostEffect = true;
                    Debug.Log("start remote");
                }
                (float b, Vector3 v) = Utils.Boost(
                    delta,
                    Velocity,
                    Controller,
                    boostImpulse,
                    boostUsageSpeed,
                    boostRemainingPercentage
                );
                boostRemainingPercentage = b;
                Velocity = v;
            }

            if(startedBoostEffect && !clientJump) {
                RPC_ExecuteEffect(1);
                startedBoostEffect = false;
                Debug.Log("stop remote1");
            }


            if(boostRemainingPercentage <= 0 && startedBoostEffect) {
                RPC_ExecuteEffect(1);
                                Debug.Log("stop remote2");

                startedBoostEffect = false;
            }



            /*
            * Move
            */
          
            (float b2, Vector3 v2) = Utils.Move(
                delta,
                movement,
                transform,
                Velocity,
                Controller,
                boostRemainingPercentage,
                gravity,
                braking,
                acceleration,
                maxBallWalkingSpeed,
                maxSprintGroundSpeed,
                maxGroundSpeed,
                maxVerticalSpeed,
                clientSprint,
                boostUsageSpeed,
                ballGunController.isCarrying
            );
            Velocity = v2;
            boostRemainingPercentage = b2;
            
            if(
                (MatchController.instance.state == State.ScoredCountDown || MatchController.instance.state == State.ScoredReset) &&
                !MatchController.instance.IsPlayerOnOwnHalf(transform.position, ballGunController.playerController.team) &&
                ballGunController.ball != null
            ) {
                Velocity = Utils.PushToOwnHalf(
                    transform,
                    delta,
                    ballGunController.playerController.team,
                    Velocity,
                    ballGunController.ball.transform.position,
                    Controller,
                    maxBallWalkingSpeed,
                    MatchController.instance.mapInfo.Value.middleCircleRadius
                );

            }

            /*
            * Recharge boost
            */
            if(
                Controller.isGrounded &&
                (!clientSprint || movement.magnitude == 0 || ballGunController.isCarrying)
            ) {
                boostRemainingPercentage = Utils.RechargeBoost(
                    delta,
                    Controller,
                    boostRemainingPercentage,
                    boostRechargeSpeed
                );
            }
                        


            /*
            * Hit ground
            */
            if(
                clientHitGround &&
                !hitGroundReceived
            ) {
                hitGroundReceived = true;
            }


            /*
            * Reset hit ground
            */
            if(
                previousClientHitGround == true &&
                !clientHitGround
            ) {
                hitGroundReceived = false;
            }


            /*
            * Accept/refuse client state
            */
            if(
                receivedInput && 
                Vector3.Distance(clientPosition, transform.position) < maxAllowedClientPositionError &&
                (Vector3.Distance(clientVelocity, Velocity) < maxAllowedClientVelocityError) &&
                Math.Abs(boostRemainingPercentage - clientBoostRemaining) < maxAllowedClientBoostError
            
            ) {
                Teleport(clientPosition);
                Velocity = clientVelocity;
                boostRemainingPercentage = clientBoostRemaining;
            }

            if(
                Velocity.magnitude > bashPlayerMinimumSpeed
            ) {
                foreach (var player in players) {
                    if(player.playerController == ballGunController.playerController || player.team == ballGunController.playerController.team) {
                        continue;
                    }
                    Vector3 otherPlayerPosition = player.playerController.transform.position;
                    Vector3 playerPosition = transform.position;
                    Vector3 playerToOtherPlayer = otherPlayerPosition - playerPosition;
                    
                    if(playerToOtherPlayer.magnitude > bashPlayerMaximumDistance) {
                        continue;
                    }
                    
                    float angle = Vector3.Angle(playerToOtherPlayer.normalized, Velocity.normalized);

                    if(angle > bashPlayerMaximumAngle) {
                        continue;
                    }
                    bool knockedOut = Velocity.magnitude >= bashPlayerKnockoutSpeed;
                    player.playerController.localCharacterMovementController.networkMovementController.Push(Velocity.normalized * bashPlayerPushStrength);
                    Push(-Velocity.normalized * (Velocity.magnitude + bashPlayerPushStrength));

                    if(player.ballGunController.isCarrying) {
                        player.ballGunController.isCarrying = false;
                        player.ballGunController.ball.Shoot(-Velocity.normalized * bashPlayerBallDropSpeed, new Vector2(), 0);
                        player.playerController.temporarilyIgnored = true;
                        ballGunController.playerController.temporarilyIgnored = true;
                        if(knockedOut) {
                            player.playerController.knockedOut = true;
                        }
                    }


                }
            }

            previousClientJump = clientJump;
            previousClientDash = clientDash;
            previousClientBoostRemaining = clientBoostRemaining;
            previousClientHitGround = clientHitGround;
        }
    }


    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All, InvokeLocal = true, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_ExecuteEffect(int effectType, RpcInfo info = default){
        if(!Object.HasInputAuthority) {
            switch(effectType) {
                case 0: {
                    boostEffect.SendEvent("StartContinuousWorldSpace");
                    break;
                }
                case 1: {
                    boostEffect.SendEvent("StopContinuousWorldSpace");
                    break;
                }
                case 2: {
                    boostEffect.SendEvent("Burst");
                    break;
                }
                case 3: {
                    jumpEffect.SendEvent("Burst");
                    break;
                }
                default: {
                    break;
                }
            }
        }
    }


    public void Push(Vector3 force) {
        Velocity += force;
        pushed = true;
    }

    public void Teleport(Vector3 position) {
        Controller.enabled = false;
        transform.position = position;
        Controller.enabled = true;
    }

    public void Update() {
        Utils.Rotate(transform, cameraController.transform.localEulerAngles.y);
    }


    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);
        unsubscribePlayers();
    }

}
