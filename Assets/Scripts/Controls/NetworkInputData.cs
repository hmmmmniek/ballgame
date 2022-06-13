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
    
    public float jumpPressedTime;
    public float primaryPressedTime;
    public float secondaryPressedTime;
    public float jumpReleaseTime;
    public float primaryReleaseTime;
    public float secondaryReleaseTime;
    public float runnerTime;
    public float localTime;

}
