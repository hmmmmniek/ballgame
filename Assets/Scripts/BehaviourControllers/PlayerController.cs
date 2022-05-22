using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;

[RequireComponent(typeof(CharacterMovementController))]
public class PlayerController : NetworkBehaviour, IPlayerLeft {
    public static PlayerController Local { get; set; }

    public Transform playerModel;
    public Transform glassesModel;

    // Start is called before the first frame update
    void Start() {

    }

    public override void Spawned() {
        if (Object.HasInputAuthority) {
            Local = this;

            Utils.SetRenderLayerDeep(playerModel, LayerMask.NameToLayer("LocalPlayerModel"));
            Utils.SetRenderLayerDeep(glassesModel, LayerMask.NameToLayer("LocalPlayerModel"));

            Debug.Log("Spawned local player");
        } else {
            Camera localCamera = GetComponentInChildren<Camera>();
            localCamera.enabled = false;

            AudioListener audioListener = GetComponentInChildren<AudioListener>();
            audioListener.enabled = false;

            Debug.Log("Spawned remote player");
        }
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