using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.UIElements;

public class NetworkStateData {
    public bool joined;
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

    public async Task Join() {
        if(state.joined == false) {
            var result = await dependencies.sessionManager.Join();
            if (result) {
                StateChange((NetworkStateData state) => {
                    state.joined = true;
                });
            }
        }
    }

    public async Task Leave() {
        if(state.joined == true) {
            var result = await dependencies.sessionManager.Leave();
            if (result) {
                StateChange((NetworkStateData state) => {
                    state.joined = false;
                });
            }
        }
    }
}