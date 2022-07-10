using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;

public class HostMigration : Fusion.Behaviour, INetworkRunnerCallbacks {



    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }


    public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) {
        NetworkManager networkManager = FindObjectOfType<NetworkManager>();
        await networkManager.JoinHostMigration(hostMigrationToken, HostMigrationResume);
    }
    void HostMigrationResume(NetworkRunner runner) {
        PlayerSessionManager playerSessionManager = runner.GetComponent<PlayerSessionManager>();
        IEnumerable<NetworkObject> networkObjects = runner.GetResumeSnapshotNetworkObjects();

        MapController mapController = null;
        foreach (var resumeNO in networkObjects) {
            if(resumeNO.TryGetBehaviour<MapController>(out var m)) {
                mapController = runner.Spawn(
                    prefab: resumeNO,
                    position: new Vector3(0, 0, 0),
                    rotation: Quaternion.LookRotation(new Vector3(0, 0, 0)),
                    onBeforeSpawned: (runner, newNO) => {
                        newNO.CopyStateFrom(resumeNO);
                    }
                ).GetComponent<MapController>();                
            }
        }

        BallController ballController = null;
        foreach (var resumeNO in networkObjects) {
            if(resumeNO.TryGetBehaviour<BallController>(out var b)) {
                ballController = runner.Spawn(
                    prefab: resumeNO,
                    position: b.ReadPosition(),
                    rotation: b.ReadRotation(),
                    onBeforeSpawned: (runner, newNO) => {
                        newNO.CopyStateFrom(resumeNO);
                    }
                ).GetComponent<BallController>();                
            }
        }
        MatchController matchController = null;
        foreach (var resumeNO in networkObjects) {
            if(resumeNO.TryGetBehaviour<MatchController>(out var m)) {
                matchController = runner.Spawn(
                    prefab: resumeNO,
                    position: new Vector3(0, 0, 0),
                    onBeforeSpawned: (runner, newNO) => {
                        newNO.CopyStateFrom(resumeNO);
                        newNO.GetComponent<MatchController>().players = new PlayerRefSet();
                        newNO.GetComponent<MatchController>().ball = ballController;
                        newNO.GetComponent<MatchController>().map = mapController;
                    }
                ).GetComponent<MatchController>();
            }
        }
        foreach (var resumeNO in networkObjects) {
            if(resumeNO.TryGetBehaviour<PlayerController>(out var p)) {
                if(!p.isHost) {
                    PlayerController playerController = runner.Spawn(
                        prefab: resumeNO,
                        position: p.ReadPosition(),
                        rotation: p.ReadRotation(),
                        onBeforeSpawned: (runner, newNO) => {
                            newNO.CopyStateFrom(resumeNO);
                            newNO.GetComponent<PlayerController>().inputAuthority = PlayerRef.None;
                            newNO.GetComponent<PlayerController>().isHost = false;
                        }
                    ).GetComponent<PlayerController>();

                    GameState.Dispatch(GameState.AddPlayer, new Player(playerController.hwid, playerController.playerName, null, playerController.ballGunController, playerController, playerController.team, false), () => {});

                }
            }
            
        }

    }
}
