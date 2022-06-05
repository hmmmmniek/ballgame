using UnityEngine;
using Fusion;
using System;
using System.Linq;

public class BallController : NetworkTransform {
    public float pickupDistance = 2f;

    public const uint NOT_ATTACHED = uint.MaxValue;
    [Networked]
    public uint attachedToPlayer {get; set;}
    [Networked]
    public uint temporarilyIgnorePlayer {get; set;}

    private BallGunController attachedToPlayerController;

    private Action unsubscribePlayers;
    private (BallGunController ballGunController, PlayerController playerController)[] players;

    public override void Spawned() {
        base.Spawned();
        attachedToPlayer = NOT_ATTACHED;
        unsubscribePlayers = GameState.Select<(BallGunController ballGunController, PlayerController playerController)[]>(GameState.GetPlayers, (players) => {
            if (players != null) {
                this.players = players;
                foreach (var player in players) {
                    player.ballGunController.ball = this;
                    player.playerController.ball = this;
                }
                bool hasCarryingPlayer = Array.Exists(players, item => {
                    return attachedToPlayer == item.ballGunController.Id.Object.Raw;
                });
                if(!hasCarryingPlayer) {
                    DetachFromPlayer();
                }
            }

        });
    }

    public override void FixedUpdateNetwork() {
        base.FixedUpdateNetwork();
        if(players != null && attachedToPlayer == NOT_ATTACHED) {
            (BallGunController ballGunController, float distance)? closest = null;
            foreach (var player in players) {
                
                float distance = Vector3.Distance(transform.position, player.playerController.transform.position);
                if(temporarilyIgnorePlayer == player.ballGunController.Id.Object.Raw && distance > pickupDistance) {
                    temporarilyIgnorePlayer = NOT_ATTACHED;
                }
                if (closest == null || closest.Value.distance > distance) {
                    closest = (ballGunController: player.ballGunController, distance: distance);
                }

            }
            if(closest.Value.distance < pickupDistance) {
                if(closest.Value.ballGunController.Id.Object.Raw != temporarilyIgnorePlayer) {
                    AttachToPlayer(closest.Value.ballGunController);
                }
            }
        }
        if(attachedToPlayer == NOT_ATTACHED && transform.parent != null) {
            DetachFromPlayer();
        }

    }

    public void AttachToPlayer(BallGunController player) {
        Debug.Log("attach");
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        rigidBody.detectCollisions = false;
        rigidBody.useGravity = false;
        rigidBody.velocity = new Vector3();
        rigidBody.angularVelocity = new Vector3();
        transform.parent = player.transform;
        transform.localPosition = new Vector3(0f, -0.3f, 0.45f);
        attachedToPlayer = player.Id.Object.Raw;

        if(player.isLocalPlayer) {
            attachedToPlayerController = player;
            player.ShowBall();
            Utils.SetRenderLayerDeep(transform, LayerMask.NameToLayer("LocalPlayerModel"));
        }
    }

    private void DetachFromPlayer() {
        transform.parent = null;
        GetComponent<Rigidbody>().detectCollisions = true;
        GetComponent<Rigidbody>().useGravity = true;
        if(gameObject.layer == LayerMask.NameToLayer("LocalPlayerModel")) {
            attachedToPlayerController.HideBall();
            attachedToPlayerController = null;
            Utils.SetRenderLayerDeep(transform, LayerMask.NameToLayer("Default"));
        }        
    }

    public void Shoot(Vector3 forward) {
        temporarilyIgnorePlayer = attachedToPlayer;
        attachedToPlayer = NOT_ATTACHED;
        DetachFromPlayer();
        GetComponent<Rigidbody>().velocity = forward;
    }


    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        unsubscribePlayers();
    }
}

