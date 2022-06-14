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
    public float clientBoostRemaining;
    public float clientChargeTime;
    public bool clientShoot;
    public bool clientKick;
    public bool clientPass;
    public bool clientSuck;
    public bool clientJump;
    public float runnerTime;
    public float localTime;

}
