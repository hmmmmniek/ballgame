using System;
using Fusion;
using UnityEngine;

public class BodyTrackingController: MonoBehaviour {
    public PlayerController playerController;
    public Transform bodyAnchorPoint;
    private bool hasInputAuthority;
    public void Init(bool hasInputAuthority) {
        this.hasInputAuthority = hasInputAuthority;
    }

    public void Update() {

        /*
        * Move model to position when not in control
        */
        if(!hasInputAuthority) {
            transform.position = bodyAnchorPoint.position;
            transform.rotation = bodyAnchorPoint.rotation;
            return;
        }
        /*
        * Move model to position when knocked out
        */
       
        if(playerController.knockedOut) {
            transform.position = bodyAnchorPoint.position;
            transform.rotation = bodyAnchorPoint.rotation;

            return;

        }

    }
}