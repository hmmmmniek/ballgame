using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkStateData {
    public bool joined;
    public List<SessionInfo> sessionList;
    public SessionInfo currentSession;
}

public class NetworkState: BaseState<NetworkStateData> {
    private StateDependencies dependencies;

    public NetworkState(StateDependencies dependencies): base() {
        this.dependencies = dependencies;

        state.joined = false;
    }
    
    public void E_GetJoined(Action<bool> callback) {
        StateSelect<bool>((NetworkStateData state) => {
            return state.joined;
        }, callback);
    }

    public void E_GetSessionList(Action<List<SessionInfo>> callback) {
        StateSelect<List<SessionInfo>>((NetworkStateData state) => {
            return state.sessionList;
        }, callback);
    }


    public void SetSessionList(List<SessionInfo> sessionList) {
        StateChange((NetworkStateData state) => {
            state.sessionList = sessionList;
        });
    }

    public async Task Join(SessionInfo session) {
        if(state.joined == false) {
            var result = await dependencies.networkManager.JoinSession(session);
            if (result) {
                StateChange((NetworkStateData state) => {
                    state.joined = true;
                    state.currentSession = session;
                });
                ViewManager.instance.Open<GameController>();
            }
        }
    }

    public async Task Leave() {
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