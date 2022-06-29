using UnityEngine;
using Fusion;
using System;
using System.Linq;
using System.Collections.Generic;

public class BallController : NetworkRigidbody {
    public float pickupDistance = 2f;
    public float maxSpeed = 80;
    public float velocityMeasuringTimeFrame = 0.3f;
    public int velocityMeasurementsAmount = 5;
    public float maxPickupForce = 20f;
    public float spinAirMaxForce = 5f;
    public float spinAirSpeedInfluence = 1f;
    public float spinVerticalSpeedInfluence = 1f;
    public float spinGroundMaxForce = 10;
    public float spinGroundMaxSpeed = 8;
    public float spinGroundFullGripVelocity = 20f;
    public float spinGroundMinGrip = 0.1f;
    public float spinGroundEffectTollerance = 0.1f;
    public float spinMaxAngularVelocity = 1000f;
    public float spinForceCapacity = 100;
    public float spinRollAirDepletionRate = 0.01f;
    public float rollingResistance = 0.01f;
    public Transform ballModel;

    [HideInInspector][Networked] public NetworkBool isAttached {get; set;}
    [HideInInspector][Networked] public NetworkTransform anchor {get; set;}
    [HideInInspector][Networked] public Vector2 spinInput {get; set;}
    [HideInInspector][Networked] public float rollInput {get; set;}
    [Networked] private Vector3 spinInitialForward { get; set; }
    [HideInInspector] public Rigidbody rigidBody;
    [Networked] private float spinCapacityLeft { get; set; }

    private Action unsubscribePlayers;
    private bool isColliding;
    private float collidingSpeed;
    private float circumference;

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
        return VelocityMeasurements.Count > 0 ? VelocityMeasurements.Average() : 0;
    }


    public override void Spawned() {
        base.Spawned();

        GameState.Dispatch(GameState.SetBall, this, () => {});

        rigidBody = GetComponent<Rigidbody>();
        rigidBody.maxAngularVelocity = spinMaxAngularVelocity * 10;

        circumference = (float)Math.PI * 2f * GetComponent<SphereCollider>().radius;

        unsubscribePlayers = GameState.Select<Player[]>(GameState.GetPlayers, (players) => {
            if (players != null) {
                Player[] activePlayers = players.Where((p) => p.playerController != null && p.team.HasValue).ToArray();
                foreach (var player in activePlayers) {
                    player.ballGunController.ball = this;
                    player.playerController.ball = this;
                    Physics.IgnoreCollision(GetComponent<SphereCollider>(), player.playerController.GetComponent<CharacterController>());
                    Physics.IgnoreCollision(GetComponent<SphereCollider>(), player.playerController.localCharacterMovementController.GetComponent<CharacterController>());
                    Physics.IgnoreCollision(player.playerController.localCharacterMovementController.GetComponent<CharacterController>(), player.playerController.GetComponent<CharacterController>());

                }
                if(Object.HasStateAuthority) {

                    bool hasCarryingPlayer = Array.Exists(activePlayers, item => {
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
        Vector3 surfaceNormal = GetNearestSurfaceNormal();
        collidingSpeed = getVelocity() * (1f - (float)Math.Pow(10000, -(1f - Vector3.Cross(surfaceNormal, rigidBody.velocity.normalized).magnitude)));
    }

    private bool previousIsColliding;
    public override void FixedUpdateNetwork() {

        base.FixedUpdateNetwork();
        if(Object.HasStateAuthority) {
            
            if(!isAttached) {
                Collider[] area = Physics.OverlapSphere(transform.position, pickupDistance);
                foreach (var item in area) {
                    PlayerController player = item.GetComponent<PlayerController>();
                    if(player != null && !player.temporarilyIgnored && !player.ballGunController.shielding && !player.knockedOut) {
                        float speed = getVelocity();
                        player.GetComponent<CharacterMovementController>().Push(rigidBody.velocity.normalized * (maxPickupForce * (speed / maxSpeed)));
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

        if(isColliding) {
            Vector3 nearestSurfaceNormal = GetNearestSurfaceNormal();
            isColliding = nearestSurfaceNormal.magnitude > 0;
        }

        if((spinInput.magnitude > 0.05 || Math.Abs(rollInput) > 0.05) && spinCapacityLeft > 0) {
            (Vector3 airForce, Vector3 groundForce, Vector3 angularVelocity) = GetSpinEffect();

            rigidBody.AddForce(Vector3.ClampMagnitude(airForce, spinCapacityLeft), ForceMode.Impulse);
            spinCapacityLeft = spinCapacityLeft - airForce.magnitude;
            if(spinCapacityLeft < 0) {
                spinCapacityLeft = 0;
            }
            rigidBody.angularVelocity = angularVelocity;
            if(collidingSpeed > 0) {
                float x = Math.Clamp(collidingSpeed / spinGroundFullGripVelocity, spinGroundMinGrip, 1);
                Vector3 appliedGroundForce = groundForce * x;
                rigidBody.AddForce(Vector3.ClampMagnitude(appliedGroundForce, spinCapacityLeft), ForceMode.Impulse);
                if(getVelocity() * (1f - Vector3.Cross(appliedGroundForce.normalized, rigidBody.velocity.normalized).magnitude) < spinGroundMaxSpeed) {
                    rigidBody.AddForce(Vector3.ClampMagnitude(appliedGroundForce, spinCapacityLeft), ForceMode.Impulse);
                }
                spinCapacityLeft = spinCapacityLeft - appliedGroundForce.magnitude;
                if(spinCapacityLeft < 0) {
                    spinCapacityLeft = 0;
                }
                collidingSpeed = 0;
            }
            if(isColliding) {

                Vector3 appliedGroundForce = groundForce * spinGroundMinGrip;
                if(getVelocity() * (1f - Vector3.Cross(appliedGroundForce.normalized, rigidBody.velocity.normalized).magnitude) < spinGroundMaxSpeed) {
                    rigidBody.AddForce(Vector3.ClampMagnitude(appliedGroundForce, spinCapacityLeft), ForceMode.Impulse);
                }

                spinCapacityLeft = spinCapacityLeft - appliedGroundForce.magnitude;
                if(spinCapacityLeft < 0) {
                    spinCapacityLeft = 0;
                }

            } else {
                if(!(spinInput.magnitude > 0.05) && Math.Abs(rollInput) > 0.05) {
                    spinCapacityLeft = spinCapacityLeft - spinRollAirDepletionRate;
                    if(spinCapacityLeft < 0) {
                        spinCapacityLeft = 0;
                    }
                }
            }

            
        }

        rigidBody.AddForce(GetRollingResistance(), ForceMode.Impulse);

        if(spinCapacityLeft == 0) {
            Vector3 nearestSurfaceNormal = GetNearestSurfaceNormal();
            if(nearestSurfaceNormal.magnitude > 0) {
                rigidBody.angularVelocity = Vector3.Cross(nearestSurfaceNormal, rigidBody.velocity.normalized) * (getVelocity() / circumference) * (float)Math.PI * 2f;
            }
            
        }

        MeasureVelocity();

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
            this.spinCapacityLeft = spinForceCapacity;
        } else {
            this.spinInput = new Vector3();
            this.rollInput = 0;
            this.spinCapacityLeft = 0;
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

        Vector3 angularVelocity = new Vector3();
        Vector3 airForce = new Vector3();
        Vector3 groundForce = new Vector3();
        Vector3 nearestSurfaceNormal = GetNearestSurfaceNormal();

        if(spinInput.magnitude > 0) {
            float ballVelocity =  getVelocity() / maxSpeed;
            float spinAirStrength = Math.Clamp((spinInput.magnitude * spinAirMaxForce) * ((float)Math.Pow(ballVelocity, spinAirSpeedInfluence)), 0, spinAirMaxForce);

            Vector3 horizontal = Vector3.Cross(Vector3.down, rigidBody.velocity.normalized).normalized;
            Vector3 vertical = Quaternion.AngleAxis(-90, rigidBody.velocity.normalized) * horizontal;
            Vector3 magnusForce = ((horizontal * spinInput.x * 0.5f) + (vertical * spinInput.y)).normalized;
            Vector3 speedChangeForce = -rigidBody.velocity.normalized * Math.Clamp(Vector3.Dot(Vector3.up, magnusForce), 0, 1) * spinVerticalSpeedInfluence;
            Vector3 spinAngularAxis = -Vector3.Cross(((horizontal * spinInput.x) + (vertical * spinInput.y)).normalized, spinInitialForward).normalized;

            angularVelocity = spinAngularAxis * spinInput.magnitude;
            airForce = (magnusForce + speedChangeForce) * spinAirStrength;


            groundForce = Vector3.Cross(nearestSurfaceNormal, -(-Vector3.Cross(((-horizontal * spinInput.x) + (vertical * spinInput.y)).normalized, spinInitialForward).normalized) * spinInput.magnitude) * spinGroundMaxForce;

        } else if(Math.Abs(rollInput) > 0) {
            angularVelocity = spinInitialForward * rollInput;
            groundForce = Vector3.Cross(nearestSurfaceNormal, -angularVelocity) * spinGroundMaxForce;
        }
        

        return (
            airForce: airForce,
            groundForce: groundForce,
            angularVelocity: angularVelocity * spinMaxAngularVelocity
        );
    }

    private Vector3 GetNearestSurfaceNormal() {
        SphereCollider sphereCollider = GetComponent<SphereCollider>();
        Collider[] objects = Physics.OverlapSphere(transform.position, sphereCollider.radius + spinGroundEffectTollerance);
        Vector3 closestObject = new Vector3();
        foreach (var collider in objects) {
            
            if(collider != sphereCollider && collider.GetComponent<CharacterController>() == null) {
                
                Vector3 ballToObject = collider.ClosestPoint(transform.position) - transform.position;
                if(closestObject.magnitude == 0 || ballToObject.magnitude < closestObject.magnitude) {
                    closestObject = ballToObject;
                }
            }
        }
        RaycastHit? closestHit = null;
        if(closestObject.magnitude > 0) {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, closestObject, sphereCollider.radius + spinGroundEffectTollerance);
            foreach (var hit in hits) {
                if(hit.collider != sphereCollider && hit.collider.GetComponent<CharacterController>() == null) {
                    closestHit = hit;
                }
            }
        }
        if(closestHit.HasValue) {
            return closestHit.Value.normal.normalized;
        } else {
            return new Vector3();
        }
        
    }


    private Vector3 GetRollingResistance() {
        Vector3 nearestSurfaceNormal = GetNearestSurfaceNormal();
        if(nearestSurfaceNormal.magnitude != 0) {
            return -rigidBody.velocity.normalized * rollingResistance;
        } else {
            return new Vector3();
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        unsubscribePlayers();
    }
}

