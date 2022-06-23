using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;

public class PlayerController : NetworkBehaviour, IPlayerLeft {
    public static PlayerController Local { get; set; }

    public Transform playerModel;
    public Transform glassesModel;
    public Transform ballModel;

    public BallGunController ballGunController;
    public LocalCharacterMovementController localCharacterMovementController;

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

    [HideInInspector] public BallController ball;
    // Start is called before the first frame update
    void Start() {

    }

    public override void Spawned() {
        base.Spawned();

        if (Object.HasInputAuthority) {
            Local = this;

            Utils.SetRenderLayerDeep(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));
            Utils.SetRenderLayerDeep(glassesModel, LayerMask.NameToLayer("LocalPlayerModel"));
            Utils.SetRenderLayerDeep(ballModel, LayerMask.NameToLayer("LocalPlayerModel"));

            GameObject.Find("Main Camera").GetComponent<Camera>().enabled = false;


        } else {
            Camera localCamera = GetComponentInChildren<Camera>();
            if(localCamera) {
                localCamera.enabled = false;
            }

            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            if(audioListener) {
                audioListener.enabled = false;
            }

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
                BallController b = item.GetComponent<BallController>();
                if(b != null) {
                    ballFound = true;
                    return;
                }
            }
            if(!ballFound) {
                temporarilyIgnored = false;
            }
        }

    }

    public void OnDestroy (){
        GameObject.Destroy(localCharacterMovementController.gameObject);
    }

}