using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;
using Fusion.Photon.Realtime;

public class NetworkManager : Fusion.Behaviour {
    public NetworkRunner runnerPrefab;
    private NetworkRunner runner;
    public async void Start() {
        await ResetRunner();
    }

    public async Task<SessionInfo> StartSession(string name, int size) {
        await ResetRunner();
        
        var startGameArgs = new StartGameArgs {
            GameMode = GameMode.Host,
            SessionName = name,
            PlayerCount = size,
            SceneObjectProvider = GetSceneObjectProvider(),
            DisableClientSessionCreation = true,
        };
        var result = await runner.StartGame(startGameArgs);

        if(result.Ok) {
            return runner.SessionInfo;
        } else {
            Debug.Log(result.ShutdownReason);
            return null;
        }
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

    public async Task ResetRunner() {
        if(runner != null) {
            Debug.Log("Shutdown");
            await runner.Shutdown(true, ShutdownReason.Ok);
        }
        runner = Instantiate(GetComponent<NetworkManager>().runnerPrefab);
        runner.ProvideInput = true;
        await runner.JoinSessionLobby(SessionLobby.ClientServer);

    }
}
