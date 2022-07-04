using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;

public class PlayerSessionManager : Fusion.Behaviour, INetworkRunnerCallbacks {
    public MatchController matchManagerPrefab;
    [HideInInspector]public MatchController matchManager;

    private float lastRtt = -1;

    public void OnConnectedToServer(NetworkRunner runner) {}
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {
        NetworkState.Dispatch(NetworkState.SetSessionList, sessionList, () => {});

    }


    public async void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {

        if (runner.LocalPlayer == player && runner.IsServer && matchManager == null) {
            matchManager = runner.Spawn(matchManagerPrefab, new Vector3(0, 0, 0));
        }

        if(runner.LocalPlayer == player) {
            string hwid = Utils.GetCurrentProcessId();
            await MatchController.PlayerJoined(player, hwid);
        }
        
    }





    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {
        if (runner.IsServer) {
            matchManager.HandlePlayerDisconnected(player);
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {
        if(PlayerController.Local != null) {
            InputHandler.instance.networkInputDataCache.localTime = Time.time;
            InputHandler.instance.networkInputDataCache.runnerTime = runner.SimulationTime;
            InputHandler.instance.networkInputDataCache.clientPosition = PlayerController.Local.localCharacterMovementController.transform.position;
            InputHandler.instance.networkInputDataCache.clientVelocity = PlayerController.Local.localCharacterMovementController.Velocity;
            InputHandler.instance.networkInputDataCache.clientBoostRemaining = PlayerController.Local.localCharacterMovementController.boostRemainingPercentage;
            InputHandler.instance.networkInputDataCache.clientJump = PlayerController.Local.localCharacterMovementController.localJump;
            InputHandler.instance.networkInputDataCache.clientSprint = PlayerController.Local.localCharacterMovementController.localSprint;
            InputHandler.instance.networkInputDataCache.clientDash = PlayerController.Local.localCharacterMovementController.localDash;
            InputHandler.instance.networkInputDataCache.clientHitGround = PlayerController.Local.localCharacterMovementController.localHitGround;
            InputHandler.instance.networkInputDataCache.clientChargeTime = PlayerController.Local.ballGunController.localChargeTime;
            InputHandler.instance.networkInputDataCache.clientShoot = PlayerController.Local.ballGunController.localShoot;
            InputHandler.instance.networkInputDataCache.clientKick = PlayerController.Local.ballGunController.localKick;
            InputHandler.instance.networkInputDataCache.clientPass = PlayerController.Local.ballGunController.localPass;
            InputHandler.instance.networkInputDataCache.clientSuck = PlayerController.Local.ballGunController.localSuck;
            InputHandler.instance.networkInputDataCache.clientShield = PlayerController.Local.ballGunController.localShield;
            InputHandler.instance.networkInputDataCache.clientBallRoll = PlayerController.Local.ballGunController.localBallRoll;
            InputHandler.instance.networkInputDataCache.clientBallSpin = PlayerController.Local.ballGunController.localBallSpin;
            InputHandler.instance.networkInputDataCache.clientBallSpinRotationStart = PlayerController.Local.ballGunController.localSpinRotationInputStart;
            InputHandler.instance.networkInputDataCache.pushedReceived = PlayerController.Local.localCharacterMovementController.networkMovementController.pushedReceived;            
            input.Set(InputHandler.instance.networkInputDataCache);
        }
    }



}

