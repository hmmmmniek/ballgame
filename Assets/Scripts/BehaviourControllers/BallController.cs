using UnityEngine;
using Fusion;
using System;
using System.Linq;
using System.Collections.Generic;

public class BallController : NetworkTransform {
    public float pickupDistance = 2f;
    public float maxSpeed = 80;
    public float velocityMeasuringTimeFrame = 0.3f;
    public int velocityMeasurementsAmount = 5;

    public float spinAirMaxForce = 5f;
    public float spinAirSpeedInfluence = 1f;
    public float spinVerticalSpeedInfluence = 1f;
    public float spinGroundMaxForce = 10;
    public float spinGroundMaxVelocity = 5f;
    public float spinGroundFullGripVelocity = 20f;
    public float spinGroundMinGrip = 0.1f;
    public float spinTime = 3f;
    public float spinMaxAngularVelocity = 1000f;

    public Transform ballModel;

    [HideInInspector][Networked] public NetworkBool isAttached {get; set;}
    [HideInInspector][Networked] public NetworkTransform anchor {get; set;}
    [HideInInspector][Networked] public Vector2 spinInput {get; set;}
    [HideInInspector][Networked] public float rollInput {get; set;}
    [Networked] private TickTimer spinTimer { get; set; }
    [Networked] private Vector3 spinInitialForward { get; set; }
    public Rigidbody rigidBody;

    private Action unsubscribePlayers;
    private (BallGunController ballGunController, PlayerController playerController)[] players;
    private bool isColliding;
    private float collidingSpeed;

    private Queue<float> VelocityMeasurements = new Queue<float>();

    private float lastMeasured = 0;
    private void MeasureVelocity() {
        float time = Time.time;

        if(time - lastMeasured > velocityMeasuringTimeFrame/velocityMeasurementsAmount) {
            lastMeasured = time;
            VelocityMeasurements.Enqueue(rigidBody.velocity.magnitude);
            if(VelocityMeasurements.Count > velocityMeasurementsAmount) {
                VelocityMeasurements.Dequeue();
            }
        }
    }
    public float getVelocity() {
        return VelocityMeasurements.Average();
    }


    public override void Spawned() {
        base.Spawned();
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.maxAngularVelocity = spinMaxAngularVelocity;

        unsubscribePlayers = GameState.Select<(BallGunController ballGunController, PlayerController playerController)[]>(GameState.GetPlayers, (players) => {
            if (players != null) {
                this.players = players;
                foreach (var player in players) {
                    player.ballGunController.ball = this;
                    player.playerController.ball = this;
                    Physics.IgnoreCollision(GetComponent<SphereCollider>(), player.playerController.GetComponent<CharacterController>());
                    Physics.IgnoreCollision(GetComponent<SphereCollider>(), player.playerController.localCharacterMovementController.GetComponent<CharacterController>());
                    Physics.IgnoreCollision(player.playerController.localCharacterMovementController.GetComponent<CharacterController>(), player.playerController.GetComponent<CharacterController>());

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

    public virtual void OnCollisionEnter(Collision c) {
        isColliding = true;
        collidingSpeed = c.relativeVelocity.y;
    }
    public virtual void OnCollisionExit(Collision c) {
        isColliding = false;
    }


    private bool previousIsColliding;
    public override void FixedUpdateNetwork() {

        base.FixedUpdateNetwork();
        if(Object.HasStateAuthority) {
            
            if(!isAttached) {
                Collider[] area = Physics.OverlapSphere(transform.position, pickupDistance);
                foreach (var item in area) {
                    PlayerController player = item.GetComponent<PlayerController>();
                    if(player != null && !player.temporarilyIgnored && !player.ballGunController.shieldOpen) {
                        Attach(player.ballGunController.ballAnchor);
                        player.ballGunController.isCarrying = true;
                        return;
                    }            
                }
            }
        }

        if(isAttached) {
            transform.position = anchor.ReadPosition();
        }

        if(spinTimer.Expired(Runner)) {
            spinTimer = TickTimer.None;
            spinInput = new Vector2();
            rollInput = 0;
        }

        if((spinInput.magnitude > 0.05 || Math.Abs(rollInput) > 0.05) && !spinTimer.ExpiredOrNotRunning(Runner)) {
            (Vector3 airForce, Vector3 groundForce, Vector3 angularVelocity) = GetSpinEffect();
            float timeEffect = ((float)spinTimer.RemainingTime(Runner) / spinTime);
            rigidBody.AddForce(airForce * timeEffect, ForceMode.Impulse);
            rigidBody.AddTorque(angularVelocity * (float)Math.Pow(1000, -(1 - timeEffect)), ForceMode.VelocityChange);

            if(collidingSpeed > 0) {
                float x = Math.Clamp(collidingSpeed / spinGroundFullGripVelocity, spinGroundMinGrip, 1);
                rigidBody.AddForce(groundForce * x * timeEffect, ForceMode.Impulse);
                Debug.Log(x);
                collidingSpeed = 0;
            }
            if(previousIsColliding && isColliding && getVelocity() < spinGroundMaxVelocity) {
                rigidBody.AddForce(groundForce * spinGroundMinGrip * timeEffect, ForceMode.Impulse);
            }
            
        }


        MeasureVelocity();

        previousIsColliding = isColliding;

    }

    public void Update() {
        if(isAttached && ballModel.gameObject.activeSelf) {
            ballModel.gameObject.SetActive(false);
        }
        if(!isAttached && !ballModel.gameObject.activeSelf){
            ballModel.gameObject.SetActive(true);
        }

    }

     static Vector3 RotateVectorAroundAxis(Vector3 vector, Vector3 axis, float degrees)
         {
             return Quaternion.AngleAxis(degrees, axis) * vector;
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

    public void Shoot(Vector3 forward, Vector2 spinInput, float rollInput) {
        if(isAttached) {
            Detach();
        }
        if(spinInput.magnitude > 0.05 || Math.Abs(rollInput) > 0.05) {
            this.spinInput = spinInput;
            this.rollInput = rollInput;
            spinTimer = TickTimer.CreateFromSeconds(Runner, spinTime);
        } else {
            this.spinInput = new Vector3();
            this.rollInput = 0;
            spinTimer = TickTimer.None;
        }
        spinInitialForward = forward.normalized;
        rigidBody.velocity = Vector3.ClampMagnitude(forward, maxSpeed);
    }

    public void ApplyForce(Vector3 forward) {
        rigidBody.AddForce(forward, ForceMode.Impulse);
    }


    private (Vector3 airForce, Vector3 groundForce, Vector3 angularVelocity) GetSpinEffect() {

        if(spinInput.magnitude < 0.05 && Math.Abs(rollInput) < 0.05) {
            return (new Vector3(), new Vector3(), new Vector3());
        }
        float ballVelocity =  getVelocity() / maxSpeed;
        float spinAirStrength = Math.Clamp((spinInput.magnitude * spinAirMaxForce) * ((float)Math.Pow(ballVelocity, spinAirSpeedInfluence)), 0, spinAirMaxForce);
                    
        Vector3 horizontal = Vector3.Cross(Vector3.down, rigidBody.velocity.normalized).normalized;
        Vector3 vertical = Quaternion.AngleAxis(-90, rigidBody.velocity.normalized) * horizontal;
        Vector3 magnusForce = ((horizontal * spinInput.x) + (vertical * spinInput.y)).normalized;
        Vector3 speedChangeForce = -rigidBody.velocity.normalized * Math.Clamp(Vector3.Dot(Vector3.up, magnusForce), 0, 1) * spinVerticalSpeedInfluence;
        
        Vector3 spinAngularAxis = -Vector3.Cross(magnusForce, spinInitialForward).normalized;
        Vector3 spinAngularVelocity = spinAngularAxis * spinInput.magnitude;
        
        Vector3 airForce = (magnusForce + speedChangeForce) * spinAirStrength;

        Vector3 backSpinGroundForce = (-spinInitialForward).normalized * spinInput.y;
        Vector3 rollGroundForce = Vector3.Cross(Vector3.down, spinInitialForward).normalized * rollInput;
        Vector3 rollAngularVelocity = spinInitialForward * rollInput;

        Vector3 groundForce = (backSpinGroundForce + rollGroundForce).normalized * spinGroundMaxForce;
        Vector3 angularVelocity = (spinAngularVelocity + rollAngularVelocity).normalized * spinMaxAngularVelocity;

        return (
            airForce: airForce,
            groundForce: groundForce,
            angularVelocity: angularVelocity
        );
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        unsubscribePlayers();
    }
}

