using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class NetworkStatsStateData: StateData {
    public float rtt;
}

public class NetworkStatsState: BaseState<NetworkStatsStateData, NetworkStatsState> {
    public const string SELECTOR = "NetworkStats";

    private StateDependencies dependencies;

    public NetworkStatsState(StateDependencies dependencies): base() {
        this.dependencies = dependencies;

        state.rtt = 0;
    }
    
    public static float GetRtt(NetworkStatsStateData state) {
        return state.rtt;
    }


    public static void SetRtt(BaseState<NetworkStatsStateData, NetworkStatsState> s, float args, Action c) { (s as NetworkStatsState).SRTT(c, args); }
    private void SRTT(Action complete, float args) {
        StateChange((NetworkStatsStateData state) => {
            state.rtt = args;
        });
    }


}