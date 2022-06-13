using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;

public class MatchController : NetworkBehaviour {

    public BallController ballPrefab;


    public override void FixedUpdateNetwork() {

    }
    public override void Spawned() {
        base.Spawned();
        BallController ball = Runner.Spawn(ballPrefab, new Vector3(0, 4, 0), Quaternion.LookRotation(new Vector3(0, 0, 0)));




    }


}
