using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public Vector2 rotationInput;
    public Vector3 clientPosition;
    public Vector3 clientVelocity;
    public Vector3 clientBallSpinRotationStart;
    public float clientBoostRemaining;
    public float clientChargeTime;
    public bool clientShoot;
    public bool clientKick;
    public bool clientPass;
    public bool clientSuck;
    public bool clientJump;
    public bool clientDash;
    public bool clientSprint;
    public bool clientShield;
    public bool clientHitGround;
    public bool clientBallRoll;
    public bool clientBallSpin;
    public float runnerTime;
    public float localTime;
    public bool pushedReceived;
}
