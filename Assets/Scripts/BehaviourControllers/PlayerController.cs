using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;

[RequireComponent(typeof(CharacterMovementController))]
public class PlayerController : NetworkBehaviour, IPlayerLeft {
    public static PlayerController Local { get; set; }

    public Transform playerModel;
    public Transform glassesModel;
    public BallGunController ballGunController;

    [HideInInspector]
    public BallController ball;
    // Start is called before the first frame update
    void Start() {

    }

    public override void Spawned() {
        base.Spawned();

        GameState.Dispatch(GameState.AddPlayer, (
            ballGunController: ballGunController,
            playerController: this
        ), () => {});

        if (Object.HasInputAuthority) {
            Local = this;

            Utils.SetRenderLayerDeep(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));
            Utils.SetRenderLayerDeep(glassesModel, LayerMask.NameToLayer("LocalPlayerModel"));

            GameObject.Find("Main Camera").GetComponent<Camera>().enabled = false;


        } else {
            Camera localCamera = GetComponentInChildren<Camera>();
            localCamera.enabled = false;

            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;

        }
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

        }
    }

}