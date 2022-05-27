using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;

public class NetworkManager : Fusion.Behaviour {
    public NetworkRunner runnerPrefab;
    private NetworkRunner runner;
    public async void Start() {
        await ResetRunner();
    }

    public async Task<bool> JoinSession(SessionInfo session) {
        await ResetRunner();

        var startGameArgs = new StartGameArgs {
            GameMode = GameMode.Client,
            SessionName = session.Name,
            SceneObjectProvider = GetSceneObjectProvider(),
            DisableClientSessionCreation = true
        };
        
        var result = await runner.StartGame(startGameArgs);

        if(result.Ok) {
            return true;
        } else {
            Debug.Log(result.ShutdownReason);
            return false;
        }
    }

    public async Task<bool> JoinHostMigration(HostMigrationToken hostMigrationToken, Action<NetworkRunner> hostMigrationResume) {
        await ResetRunner();

        StartGameResult result = await runner.StartGame(new StartGameArgs() {
            HostMigrationToken = hostMigrationToken,
            HostMigrationResume = hostMigrationResume,
            SceneObjectProvider = GetSceneObjectProvider()
        });

        if (result.Ok) {
            return true;
        } else {
            Debug.LogWarning(result.ShutdownReason);
            return false;
        }
    }
    public async Task<bool> Leave() {
        await ResetRunner();
        return true;
    }

    private INetworkSceneObjectProvider GetSceneObjectProvider() {
        var sceneObjectProvider = GetComponent<NetworkSceneManagerDefault>();
        if(sceneObjectProvider == null) {
            sceneObjectProvider = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        return sceneObjectProvider;
    }

    private async Task ResetRunner() {
        if(runner != null) {
            await runner.Shutdown(true, ShutdownReason.Ok);
        }
        runner = Instantiate(GetComponent<NetworkManager>().runnerPrefab);
        runner.ProvideInput = true;

        await runner.JoinSessionLobby(SessionLobby.ClientServer);

    }
}
