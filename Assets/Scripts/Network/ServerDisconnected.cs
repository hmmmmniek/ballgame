using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerDisconnected : MonoBehaviour, INetworkRunnerCallbacks {
    private Camera mainCamera;

    public void Start() {
        mainCamera = Camera.main;
    }
    public async void OnDisconnectedFromServer(NetworkRunner runner) {
        await GetComponent<NetworkRunner>().Shutdown(false);

        Debug.Log("onDisconnect");
        mainCamera.enabled = true;
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
        if(shutdownReason != ShutdownReason.HostMigration) {
            Debug.Log("onShutdown");
            Destroy(GetComponent<NetworkRunner>());
            Destroy(GetComponent<NetworkSceneManagerDefault>());
            Destroy(GetComponent<NetworkPhysicsSimulation3D>());
            Destroy(GetComponent<HitboxManager>());
            mainCamera.enabled = true;
        }
    }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}

