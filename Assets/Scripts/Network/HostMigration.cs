using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;

public class HostMigration : MonoBehaviour, INetworkRunnerCallbacks {

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
        Debug.Log("onHostMigration");
        await runner.Shutdown(false, ShutdownReason.HostMigration);
        DestroyImmediate(GetComponent<NetworkRunner>());
        Destroy(GetComponent<NetworkSceneManagerDefault>());
        Destroy(GetComponent<NetworkPhysicsSimulation3D>());
        Destroy(GetComponent<HitboxManager>());

        var networkRunner = gameObject.AddComponent<NetworkRunner>();
        var sceneObjectProvider = gameObject.AddComponent<NetworkSceneManagerDefault>();

        networkRunner.ProvideInput = true;

        StartGameResult result = await networkRunner.StartGame(new StartGameArgs() {
            HostMigrationToken = hostMigrationToken,
            HostMigrationResume = HostMigrationResume,
            SceneObjectProvider = sceneObjectProvider
        });

        if (result.Ok == false) {
            Debug.LogWarning(result.ShutdownReason);
        } else {
            Debug.Log("Done");
        }
    }
    void HostMigrationResume(NetworkRunner runner) {

       /* foreach (var resumeNO in runner.GetResumeSnapshotNetworkObjects()) {

            if (resumeNO.TryGetBehaviour<NetworkPositionRotation>(out var posRot)) {
                runner.Spawn(resumeNO, position: posRot.ReadPosition(), rotation: posRot.ReadRotation(), onBeforeSpawned: (runner, newNO) => {
                    newNO.CopyStateFrom(resumeNO);
                });
            }
        }*/
    }
}
