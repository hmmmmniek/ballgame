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
    public PlayerController playerPrefab;
    public MatchController matchManagerPrefab;
    private MatchController matchManager;

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


    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        if (runner.LocalPlayer == player && runner.IsServer) {
            matchManager = runner.Spawn(matchManagerPrefab, new Vector3(0, 0, 0));
        }
        if (runner.IsServer) {
            runner.Spawn(playerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }


    public void OnInput(NetworkRunner runner, NetworkInput input) {
        if(PlayerController.Local != null) {
            InputHandler.instance.networkInputDataCache.localTime = Time.time;
            InputHandler.instance.networkInputDataCache.runnerTime = runner.SimulationTime;
            InputHandler.instance.networkInputDataCache.clientPosition = PlayerController.Local.localCharacterMovementController.transform.position;
            InputHandler.instance.networkInputDataCache.clientVelocity = PlayerController.Local.localCharacterMovementController.Velocity;
            InputHandler.instance.networkInputDataCache.clientBoostRemaining = PlayerController.Local.localCharacterMovementController.boostRemainingPercentage;
            if(InputHandler.instance.networkInputDataCache.jumpPressedTime == -1) {
                InputHandler.instance.networkInputDataCache.jumpPressedTime = runner.SimulationTime;
            }
            if(InputHandler.instance.networkInputDataCache.primaryPressedTime == -1) {
                InputHandler.instance.networkInputDataCache.primaryPressedTime = runner.SimulationTime;
            }
            if(InputHandler.instance.networkInputDataCache.secondaryPressedTime == -1) {
                InputHandler.instance.networkInputDataCache.secondaryPressedTime = runner.SimulationTime;
            }
            if(InputHandler.instance.networkInputDataCache.jumpReleaseTime == -1) {
                InputHandler.instance.networkInputDataCache.jumpReleaseTime = runner.SimulationTime;
            }
            if(InputHandler.instance.networkInputDataCache.primaryReleaseTime == -1) {
                InputHandler.instance.networkInputDataCache.primaryReleaseTime = runner.SimulationTime;
            }
            if(InputHandler.instance.networkInputDataCache.secondaryReleaseTime == -1) {
                InputHandler.instance.networkInputDataCache.secondaryReleaseTime = runner.SimulationTime;
            }
            input.Set(InputHandler.instance.networkInputDataCache);
        }
    }



}

