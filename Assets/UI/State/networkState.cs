using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;
using static CreateSessionController;

public class NetworkStateData: StateData {
    public bool joined;
    public List<SessionInfo> sessionList;
    public SessionInfo currentSession;
    public string region = Fusion.Photon.Realtime.PhotonAppSettings.Instance.AppSettings.FixedRegion;
}

public class NetworkState: BaseState<NetworkStateData, NetworkState> {
    public const string SELECTOR = "Network";

    private StateDependencies dependencies;

    public NetworkState(StateDependencies dependencies): base() {
        this.dependencies = dependencies;

        state.joined = false;
    }
    
    public static bool GetJoined(NetworkStateData state) {
        return state.joined;
    }

    public static List<SessionInfo> GetSessionList(NetworkStateData state) {
        return state.sessionList;
    }

    public static string GetRegion(NetworkStateData state) {
        return state.region;
    }

    public static MapSize? GetMapSize(NetworkStateData state) {
        if(state.currentSession == null) {
            return null;
        }
        SessionProperty mapSize;
        if(state.currentSession.Properties.TryGetValue("mapSize", out mapSize)) {
            switch ((int)mapSize) {
                case (int)MapSize.Small: {
                    return MapSize.Small;
                }
                case (int)MapSize.Medium: {
                    return MapSize.Medium;
                }
                case (int)MapSize.Large: {
                    return MapSize.Large;
                }
            }
        }
        return null;
    }


    public static void SetSessionList(BaseState<NetworkStateData, NetworkState> s, List<SessionInfo> args, Action c) { (s as NetworkState).SSL(c, args); }
    private void SSL(Action complete, List<SessionInfo> args) {
        StateChange((NetworkStateData state) => {
            state.sessionList = args;
        });
    }

    public static void SetRegion(BaseState<NetworkStateData, NetworkState> s, string args, Action c) { (s as NetworkState).SR(c, args); }
    private async void SR(Action complete, string args) {
        Fusion.Photon.Realtime.PhotonAppSettings.Instance.AppSettings.FixedRegion = args;
        StateChange((NetworkStateData state) => {
            state.region = args;
        });
        await dependencies.networkManager.ResetRunner();
    }


    public static void Create(BaseState<NetworkStateData, NetworkState> s, (string, int, MapSize) args, Action c) { (s as NetworkState).C(c, args); }
    private async void C(Action complete, (string name, int size, MapSize mapSize) args) {
        if(state.joined == false) {
            var result = await dependencies.networkManager.StartSession(args.name, args.size, args.mapSize);
            if (result) {
                StateChange((NetworkStateData state) => {
                    state.joined = true;
                    state.currentSession = result;
                });

                ViewManager.instance.Open<GameController>();
            }
        }
    }

    public static void Join(BaseState<NetworkStateData, NetworkState> s, SessionInfo args, Action c) { (s as NetworkState).J(c, args); }
    private async void J(Action complete, SessionInfo args) {
        if(state.joined == false) {
            var result = await dependencies.networkManager.JoinSession(args);
            if (result) {
                StateChange((NetworkStateData state) => {
                    state.joined = true;
                    state.currentSession = args;
                });
                ViewManager.instance.Open<GameController>();
            }
        }
    }

    public static void Leave(BaseState<NetworkStateData, NetworkState> s, object args, Action c) { (s as NetworkState).L(c); }
    private async void L(Action complete) {
        if(state.joined == true) {
            var result = await dependencies.networkManager.Leave();
            if (result) {
                StateChange((NetworkStateData state) => {
                    state.joined = false;
                    state.currentSession = null;
                });
            }
        }
    }


}