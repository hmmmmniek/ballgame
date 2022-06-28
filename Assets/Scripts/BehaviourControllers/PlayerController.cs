using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;

public class PlayerController : NetworkRigidbody, IPlayerLeft {
    public static PlayerController Local { get; set; }

    public float knockoutTime = 5f;
    public Transform playerModel;
    public Transform glassesModel;
    public Transform ballModel;
    public CapsuleCollider capsuleCollider;
    public Rigidbody rigidBody;

    public BallGunController ballGunController;
    public CharacterMovementController networkCharacterMovementController;
    public LocalCharacterMovementController localCharacterMovementController;
    public BodyTrackingController bodyTrackingController;

    private Queue<float> RttMeasurements = new Queue<float>();

    [HideInInspector][Networked] public NetworkBool temporarilyIgnored {get; set;}
    [HideInInspector][Networked(OnChanged = nameof(OnLastReceivedInputTimeChanged))] public float lastReceivedLocalTime {get; set;}
    public static void OnLastReceivedInputTimeChanged(Changed<PlayerController> changed) {
        changed.Behaviour.OnLastReceivedInputTimeChanged();
    }
    private float lastMeasured = 0;
    private void OnLastReceivedInputTimeChanged() {
        float time = Time.time;

        if(Object.HasInputAuthority & time - lastMeasured > 0.1) {
            lastMeasured = time;
            RttMeasurements.Enqueue((float)(time - lastReceivedLocalTime) * 1000);
            if(RttMeasurements.Count > 10) {
                RttMeasurements.Dequeue();
            }
            NetworkStatsState.Dispatch(NetworkStatsState.SetRtt, (float)Math.Round(RttMeasurements.Average(), 1), () => {});
        }
    }

    
    [Networked] private TickTimer knockoutTimer { get; set; }
    [HideInInspector][Networked(OnChanged = nameof(OnKnockedOutChanged))]
    public bool knockedOut { get; set; }
    public static void OnKnockedOutChanged(Changed<PlayerController> changed) {
        changed.Behaviour.OnKnockedOutChanged();
    }
    private void OnKnockedOutChanged() {
        if(knockedOut) {
            knockoutTimer = TickTimer.CreateFromSeconds(Runner, knockoutTime);
            capsuleCollider.enabled = true;
            rigidBody.isKinematic = false;
            rigidBody.velocity = localCharacterMovementController.networkMovementController.Velocity;
            rigidBody.angularVelocity = new Vector3(10, 0, 10);
            localCharacterMovementController.networkMovementController.Controller.enabled = false;
            localCharacterMovementController.networkMovementController.enabled = false;
            localCharacterMovementController.Controller.enabled = false;
            localCharacterMovementController.enabled = false;
            
        }
        if(!knockedOut) {
            capsuleCollider.enabled = false;
            rigidBody.velocity = new Vector3();
            rigidBody.angularVelocity = new Vector3();
            rigidBody.isKinematic = true;
            localCharacterMovementController.networkMovementController.Controller.enabled = true;
            localCharacterMovementController.networkMovementController.enabled = true;
            localCharacterMovementController.Controller.enabled = true;
            localCharacterMovementController.enabled = true;
            transform.rotation = new Quaternion();

        }
    }
    

    [HideInInspector] public BallController ball;
    // Start is called before the first frame update
    void Start() {

    }

    protected override void CopyFromBufferToEngine() {
        if(networkCharacterMovementController.Controller == null) {
            base.CopyFromBufferToEngine();
        } else {
            bool currentValue = networkCharacterMovementController.Controller.enabled;
            networkCharacterMovementController.Controller.enabled = false;
            base.CopyFromBufferToEngine();
            networkCharacterMovementController.Controller.enabled = currentValue;
        }
    }

    public override void Spawned() {
        base.Spawned();

        rigidBody.isKinematic = true;
        capsuleCollider.enabled = false;


        if (Object.HasInputAuthority) {
            Local = this;

            Utils.SetRenderLayerDeep(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));
            Utils.SetRenderLayerDeep(glassesModel, LayerMask.NameToLayer("LocalPlayerModel"));
            Utils.SetRenderLayerDeep(ballModel, LayerMask.NameToLayer("LocalPlayerModel"));

            GameObject.Find("Main Camera").GetComponent<Camera>().enabled = false;

            bodyTrackingController.Init(true);
        } else {
            Camera localCamera = GetComponentInChildren<Camera>();
            if(localCamera) {
                localCamera.enabled = false;
            }

            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            if(audioListener) {
                audioListener.enabled = false;
            }
            bodyTrackingController.Init(false);

        }

        transform.name = $"Player {Object.Id}";

        GameState.Dispatch(GameState.AddPlayer, (
            ballGunController: ballGunController,
            playerController: this
        ), () => {});

    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        GameState.Dispatch(GameState.RemovePlayer, (
            ballGunController: GetComponentInChildren<BallGunController>(),
            playerController: GetComponent<PlayerController>()
        ), () => {});
    }

    public void PlayerLeft(PlayerRef player) {
        if (player == Object.InputAuthority) {
            Runner.Despawn(Object);
        }

    }




    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {
            if(Object.HasStateAuthority) {
                lastReceivedLocalTime = networkInputData.localTime;
            }
        }

        if(temporarilyIgnored && Object.HasStateAuthority) {
            Collider[] area = Physics.OverlapSphere(transform.position, ball.pickupDistance + 0.5f);
            bool ballFound = false;
            foreach (var item in area) {
                if(ballFound) {
                    continue;
                }
                BallController b = item.GetComponent<BallController>();
                if(b != null) {
                    ballFound = true;
                }
            }
            if(!ballFound) {
                temporarilyIgnored = false;
            }
        }

        if(Object.HasStateAuthority && knockoutTimer.Expired(Runner)) {
            knockedOut = false;
            knockoutTimer = TickTimer.None;
        }

    }

    public bool IsLocal() {
        if(PlayerController.Local == null) {
            return false;
        } else {
            return PlayerController.Local.Id.Object.Raw == this.Id.Object.Raw;
        }
    }

    public void OnDestroy (){
        GameObject.Destroy(localCharacterMovementController.networkMovementController.cameraController.gameObject);
        GameObject.Destroy(localCharacterMovementController.networkMovementController.gameObject);
        GameObject.Destroy(localCharacterMovementController.gameObject);
    }

}