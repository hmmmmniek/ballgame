using UnityEngine;
using Fusion;
using System;
using System.Linq;

public class BallController : NetworkTransform {
    public float pickupDistance = 2f;
    public float maxSpeed = 80;

    public Transform ballModel;

    [HideInInspector][Networked] public NetworkBool isAttached {get; set;}
    [HideInInspector][Networked] public NetworkTransform anchor {get; set;}
    private Rigidbody rigidBody;

    private Action unsubscribePlayers;
    private (BallGunController ballGunController, PlayerController playerController)[] players;

    public override void Spawned() {
        base.Spawned();
        rigidBody = GetComponent<Rigidbody>();


        unsubscribePlayers = GameState.Select<(BallGunController ballGunController, PlayerController playerController)[]>(GameState.GetPlayers, (players) => {
            if (players != null) {
                this.players = players;
                foreach (var player in players) {
                    player.ballGunController.ball = this;
                    player.playerController.ball = this;
                    Physics.IgnoreCollision(GetComponent<SphereCollider>(), player.playerController.GetComponent<CharacterController>());
                }
                if(Object.HasStateAuthority) {

                    bool hasCarryingPlayer = Array.Exists(players, item => {
                        return item.ballGunController.isCarrying;
                    });
                    if(!hasCarryingPlayer && isAttached) {
                        Detach();
                    }
                }
            }
        });
    }

    public override void FixedUpdateNetwork() {

        base.FixedUpdateNetwork();
        if(Object.HasStateAuthority) {

            if(!isAttached) {
                Collider[] area = Physics.OverlapSphere(transform.position, pickupDistance);
                foreach (var item in area) {
                    PlayerController player = item.GetComponent<PlayerController>();
                    if(player != null && !player.temporarilyIgnored) {
                        Attach(player.ballGunController.ballAnchor);
                        player.ballGunController.isCarrying = true;
                        return;
                    }            
                }
            }

            if(isAttached) {
                transform.position = anchor.ReadPosition();
            }
        }


    }

    public void Update() {
        if(isAttached && ballModel.gameObject.activeSelf) {
            ballModel.gameObject.SetActive(false);
        }
        if(!isAttached && !ballModel.gameObject.activeSelf){
            ballModel.gameObject.SetActive(true);
        }
    }

    public void Attach(NetworkTransform ballAnchor) {
        if(Object.HasStateAuthority) {
            anchor = ballAnchor;
            isAttached = true;
            DisablePhysics();
        }
    }

    public void Detach() {
        if(Object.HasStateAuthority) {
            EnablePhysics();
            Collider[] area = Physics.OverlapSphere(transform.position, pickupDistance);
            foreach (var item in area) {
                PlayerController player = item.GetComponent<PlayerController>();
                if(player != null) {
                    player.temporarilyIgnored = true;
                }            
            }
            isAttached = false;
        }
    }


    public void DisablePhysics() {
        rigidBody.detectCollisions = false;
        rigidBody.useGravity = false;
        rigidBody.velocity = new Vector3();
        rigidBody.angularVelocity = new Vector3();
    }

    public void EnablePhysics() {
        rigidBody.detectCollisions = true;
        rigidBody.useGravity = true;
    }

    public void Shoot(Vector3 forward) {
        if(isAttached) {
            Detach();
        }
        rigidBody.velocity = Vector3.ClampMagnitude(forward, maxSpeed);
    }

    public void ApplyForce(Vector3 forward) {
        rigidBody.AddForce(forward, ForceMode.Impulse);
    }


    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        unsubscribePlayers();
    }
}

