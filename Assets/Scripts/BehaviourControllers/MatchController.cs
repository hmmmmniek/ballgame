using UnityEngine;
using Fusion;

public class MatchController : NetworkBehaviour {

    public BallController ballPrefab;
    public MapController mapPrefab;




    public override void FixedUpdateNetwork() {

    }
    public override void Spawned() {
        base.Spawned();
        MapController map = Runner.Spawn(mapPrefab, new Vector3(0, 0, 0), Quaternion.LookRotation(new Vector3(0, 0, 0)));
        BallController ball = Runner.Spawn(ballPrefab, new Vector3(0, 4, 0), Quaternion.LookRotation(new Vector3(0, 0, 0)));




    }


}
