using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;

public class PlayerSessionManager : MonoBehaviour, INetworkRunnerCallbacks {
    public PlayerController playerPrefab;
    private Camera mainCamera;

    public void Start() {
        mainCamera = Camera.main;
    }

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
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {
        if (runner.IsServer) {
            runner.Spawn(playerPrefab, Utils.GetRandomSpawnPoint(), Quaternion.identity, player);
        }
        if (runner.LocalPlayer == player) {
            mainCamera.enabled = false;
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input) {
        input.Set(InputHandler.instance.networkInputDataCache);
        InputHandler.instance.ResetNetworkState();
    }

    public async Task<bool> Join() {
        var result = await InitializeNetworkRunner(GameMode.AutoHostOrClient);
        if(result.Ok) {
            return true;
        } else {
            Debug.Log("Error while joining!");
            return false;
        }
    }

    public async Task<bool> Leave() {
        var networkRunner = GetComponent<NetworkRunner>();
        if(networkRunner.IsServer) {
            await networkRunner.Shutdown(false, ShutdownReason.Ok);
        } else {
            await networkRunner.Shutdown(false, ShutdownReason.Ok);
        }
        return true;
    }


    private async Task<StartGameResult> InitializeNetworkRunner(GameMode gameMode) {
        var networkRunner = gameObject.AddComponent<NetworkRunner>();
        var sceneObjectProvider = gameObject.AddComponent<NetworkSceneManagerDefault>();

        networkRunner.ProvideInput = true;
        var startGameArgs = new StartGameArgs {
            GameMode = gameMode,
            SessionName = "TestRoom",
            SceneObjectProvider = sceneObjectProvider
        };
        return await networkRunner.StartGame(startGameArgs);

    }
}
