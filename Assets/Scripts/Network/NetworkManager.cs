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
using static CreateSessionController;

public class NetworkManager : Fusion.Behaviour {
    public NetworkRunner runnerPrefab;
    private NetworkRunner runner;
    public async void Start() {
        await ResetRunner(ShutdownReason.Ok);
    }

    public async Task<SessionInfo> StartSession(string sessionName, int size, MapSize mapSize) {
        await ResetRunner(ShutdownReason.Ok);

        var result = await this.StartSimulation(
            runner: runner,
            gameMode: GameMode.Host,
            sessionName: sessionName,
            playerCount: size,
            customProps: new Dictionary<string, SessionProperty>(new KeyValuePair<string, SessionProperty>[] {
                new KeyValuePair<string, SessionProperty>("mapSize", (int)mapSize)
            })
        );
        if (result.Ok) {
            return runner.SessionInfo;
        } else {
            Debug.Log(result.ShutdownReason);
            return null;
        }
    }

    public async Task<bool> JoinSession(SessionInfo session) {
        await ResetRunner(ShutdownReason.Ok);
        
        var result = await this.StartSimulation(
            runner: runner,
            gameMode: GameMode.Client,
            sessionName: session.Name
        );

        if (result.Ok) {
            return true;
        } else {
            Debug.Log(result.ShutdownReason);
            return false;
        }
    }

    public async Task<bool> JoinHostMigration(HostMigrationToken hostMigrationToken, Action<NetworkRunner> hostMigrationResume) {
        await ResetRunner(ShutdownReason.HostMigration);

        var result = await StartSimulation(
            runner: runner,
            gameMode: hostMigrationToken.GameMode,
            migrationToken: hostMigrationToken,
            migrationResume: hostMigrationResume
        );


        if (result.Ok) {
            return true;
        } else {
            Debug.LogWarning(result.ShutdownReason);
            return false;
        }
    }
    public async Task<bool> Leave() {
        await ResetRunner(ShutdownReason.Ok);
        return true;
    }

    private INetworkSceneManager GetSceneManager() {
        var sceneObjectProvider = GetComponent<NetworkSceneManagerDefault>();
        if (sceneObjectProvider == null) {
            sceneObjectProvider = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
        return sceneObjectProvider;
    }

    public async Task ResetRunner(ShutdownReason reason) {
        if (runner != null) {
            await runner.Shutdown(true, reason);
        }
        runner = Instantiate(runnerPrefab);
        runner.ProvideInput = true;
        runner.name = "NetworkRunner";
        await runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    public Task<StartGameResult> StartSimulation(
      NetworkRunner runner,
      GameMode gameMode,
      Dictionary<string, SessionProperty> customProps = null,
      AuthenticationValues authentication = null,
      ushort port = 0,
      string customLobby = null,
      HostMigrationToken migrationToken = null,
      Action<NetworkRunner> migrationResume = null,
      Action<NetworkRunner> init = null,
      int? playerCount = null,
      string sessionName = null,
      bool disableClientSessionCreation = false
    ) {
        Debug.Log("startSimulation");
        return runner.StartGame(new StartGameArgs() {

            SessionName = sessionName,
            GameMode = gameMode,
            Scene = SceneManager.GetActiveScene().buildIndex,
            SceneManager = GetSceneManager(),
            SessionProperties = customProps,
            AuthValues = authentication,
            DisableNATPunchthrough = false,
            Address = NetAddress.Any(port),
            CustomLobbyName = customLobby,
            DisableClientSessionCreation = disableClientSessionCreation,
            HostMigrationToken = migrationToken,
            HostMigrationResume = migrationResume,
            Initialized = init,
            PlayerCount = playerCount
        });
    }
}
