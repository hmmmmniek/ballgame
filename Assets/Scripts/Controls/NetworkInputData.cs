using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public Vector2 movementInput;
    public Vector2 rotationInput;
    public NetworkBool isJumpPressed;
    public NetworkBool isFirePressed;

}
