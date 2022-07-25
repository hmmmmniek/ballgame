using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;
using System.Threading.Tasks;

public class PlayerController : NetworkRigidbody {
    public static PlayerController Local { get; set; }

    public float knockoutTime = 5f;
    public Transform playerModel;
    public Transform ballModel;
    public CapsuleCollider capsuleCollider;
    public Rigidbody rigidBody;
    public Material teamBlueMaterial;
    public Material teamRedMaterial;

    public BallGunController ballGunController;
    public CharacterMovementController networkCharacterMovementController;
    public LocalCharacterMovementController localCharacterMovementController;
    public BodyTrackingController bodyTrackingController;
    public CharacterCameraController cameraController;
    public MeshRenderer bodyMeshRenderer;
    public bool despawned = false;
    public bool initialized = false;
    private Queue<float> fpsMeasurements = new Queue<float>();
    [HideInInspector][Networked(OnChanged = nameof(OnTeamChanged))] public Team team {get; set;}
    public static void OnTeamChanged(Changed<PlayerController> changed) {
        changed.Behaviour.OnTeamChanged();
    }
    private void OnTeamChanged() {
        switch(team) {
            case Team.Blue: {
                bodyMeshRenderer.sharedMaterial = teamBlueMaterial;
                break;
            }
            case Team.Red: {
                bodyMeshRenderer.sharedMaterial = teamRedMaterial;
                break;
            }
        }
    }
    [HideInInspector][Networked(OnChanged = nameof(OnInputAuthorityChanged))] public PlayerRef inputAuthority {get; set;}
    public static void OnInputAuthorityChanged(Changed<PlayerController> changed) {
        changed.Behaviour.OnInputAuthorityChanged();
    }
    private void OnInputAuthorityChanged() {

        if(Object.HasStateAuthority) {
            Object.AssignInputAuthority(inputAuthority);
            cameraController.Object.AssignInputAuthority(inputAuthority);
            ballGunController.Object.AssignInputAuthority(inputAuthority);
            networkCharacterMovementController.Object.AssignInputAuthority(inputAuthority);
        }


        transform.name = $"#{inputAuthority.PlayerId} Player";
        Player player = new Player(hwid, playerName, inputAuthority, ballGunController, this, team, Runner.LocalPlayer == inputAuthority);
        GameState.Dispatch(GameState.UpdatePlayer, (player: player, usePlayerRef: true), () => {});

        if (Runner.LocalPlayer == inputAuthority) {
            Local = this;

            Utils.SetRenderLayerDeep(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));
            Utils.SetRenderLayerDeep(ballModel, LayerMask.NameToLayer("LocalPlayerModel"));

            GameObject.Find("Main Camera").GetComponent<Camera>().enabled = false;
            cameraController.cam.enabled = true;
            cameraController.audioListener.enabled = true;
            bodyTrackingController.Init(true, inputAuthority);
            ballGunController.localBallCamera.gameObject.SetActive(true);

            if(lastRotationInput.magnitude > 0) {
                InputHandler.instance.networkInputDataCache.rotationInput = lastRotationInput;
            }
            if(Runner.IsServer) {
                isHost = true;
            }
        }
        if(Runner.LocalPlayer != inputAuthority) {
            Utils.SetRenderLayerDeep(playerModel, LayerMask.NameToLayer("Default"));
            Utils.SetRenderLayerDeep(ballModel, LayerMask.NameToLayer("Default"));

            cameraController.cam.enabled = false;
            cameraController.audioListener.enabled = false;
            bodyTrackingController.Init(false, inputAuthority);
            ballGunController.localBallCamera.gameObject.SetActive(false);

        }

        cameraController.Init(inputAuthority);


    }
    [HideInInspector][Networked] public bool isHost {get; set;}
    [HideInInspector][Networked(OnChanged = nameof(OnHWIDChanged)), Capacity(64)] public string hwid {get; set;}
    public static void OnHWIDChanged(Changed<PlayerController> changed) {
        changed.Behaviour.OnHWIDChanged();
    }
    private void OnHWIDChanged() {
        Player player = new Player(hwid, playerName, inputAuthority, ballGunController, this, team, Runner.LocalPlayer == inputAuthority);
        GameState.Dispatch(GameState.UpdatePlayer, (player: player, usePlayerRef: true), () => {});
    }
    [HideInInspector][Networked(OnChanged = nameof(OnNameChanged)), Capacity(64)] public string playerName {get; set;}
    public static void OnNameChanged(Changed<PlayerController> changed) {
        changed.Behaviour.OnNameChanged();
    }
    private void OnNameChanged() {
        Player player = new Player(hwid, playerName, inputAuthority, ballGunController, this, team, Runner.LocalPlayer == inputAuthority);
        GameState.Dispatch(GameState.UpdatePlayer, (player: player, usePlayerRef: true), () => {});
    }
    [HideInInspector][Networked] public NetworkBool temporarilyIgnored {get; set;}
    [HideInInspector][Networked(OnChanged = nameof(OnFpsChanged))] public float fps {get; set;}
    public static void OnFpsChanged(Changed<PlayerController> changed) {
        changed.Behaviour.OnFpsChanged();
    }
    private float lastMeasured = 0;
    private void OnFpsChanged() {
        float time = Time.time;

        if(Object.HasInputAuthority & time - lastMeasured > 0.1) {
            lastMeasured = time;
            fpsMeasurements.Enqueue(fps);
            if(fpsMeasurements.Count > 10) {
                fpsMeasurements.Dequeue();
            }
            GameStatsState.Dispatch(GameStatsState.SetFps, (float)Math.Round(fpsMeasurements.Average(), 1), () => {});
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
    
    [HideInInspector][Networked]public Vector2 lastRotationInput { get; set; }


    [HideInInspector] public BallController ball;
    // Start is called before the first frame update
    void Start() {

    }


    protected override void CopyFromBufferToEngine() {
        bool oldValue = rigidBody.isKinematic;
        rigidBody.isKinematic = false;
        if(networkCharacterMovementController.Controller == null) {
            base.CopyFromBufferToEngine();
        } else {
            bool currentValue = networkCharacterMovementController.Controller.enabled;
            networkCharacterMovementController.Controller.enabled = false;
            base.CopyFromBufferToEngine();
            networkCharacterMovementController.Controller.enabled = currentValue;
        }  
        rigidBody.isKinematic = oldValue;
      
    }

    public override void Spawned() {
        base.Spawned();
        rigidBody.isKinematic = true;
        capsuleCollider.enabled = false;
        cameraController.cam.enabled = false;
        cameraController.audioListener.enabled = false;
        transform.name = $"#? Player";
        bodyTrackingController.transform.name = "#? Body";
        cameraController.transform.name = "#? Camera";

        if(Object.HasInputAuthority) {
            RPC_Joined(Runner.LocalPlayer, Utils.GetCurrentProcessId(), UserState.SelectOnce(UserState.GetPlayerName));
        }
        if(Object.InputAuthority == PlayerRef.None) {
            AutoRemove();
        }


    }
    public async void AutoRemove() {
        await Task.Delay(5000);
        if(Object.InputAuthority == PlayerRef.None) {
            MatchController.instance.HandlePlayerDisconnected(hwid);
        }
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.StateAuthority, InvokeLocal = true, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_Joined(PlayerRef playerRef, string id, string name, RpcInfo info = default){
        if(info.Source == playerRef) {
            this.hwid = id;
            this.playerName = name;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        despawned = true;
        if (IsLocal()) {

            GameObject obj = GameObject.Find("Main Camera");
            if(obj != null) {
                Camera mainCamera = obj.GetComponent<Camera>();
                if(mainCamera != null) {
                    mainCamera.enabled = true;
                }
            }
            
        }
        Player[] players = GameState.SelectOnce(GameState.GetPlayers);
        if(players.Count() > 0) {
            IEnumerable<Player> playersFiltered = players.Where(p => p.hwid == hwid);
            if(playersFiltered.Count() > 0) {
                Player localPlayer = playersFiltered.First();
                localPlayer.ballGunController = null;
                localPlayer.playerController = null;
                localPlayer.team = null;

                GameState.Dispatch(GameState.UpdatePlayer, (player: localPlayer, usePlayerRef: false), () => {});
            }


        }
        

        GameObject.Destroy(bodyTrackingController.gameObject);
        GameObject.Destroy(cameraController.gameObject);
        GameObject.Destroy(this);
    }



    private float lastRttCheck = 0;
    public override void FixedUpdateNetwork() {
        if (GetInput(out NetworkInputData networkInputData)) {
            if(Object.HasStateAuthority) {
                fps = networkInputData.localFps;
                lastRotationInput = networkInputData.rotationInput;
            }
        }
        
        if(Runner.SimulationTime - lastRttCheck > 1000) {
            NetworkStatsState.Dispatch(NetworkStatsState.SetRtt, (float)Runner.GetPlayerRtt(Object.InputAuthority), () => {});
        }


        if(!despawned && temporarilyIgnored && Object.HasStateAuthority) {
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




}