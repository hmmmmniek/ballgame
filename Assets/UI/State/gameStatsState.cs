using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class GameStatsStateData: StateData {
    public float fps;
}

public class GameStatsState: BaseState<GameStatsStateData, GameStatsState> {
    public const string SELECTOR = "GameStats";

    private StateDependencies dependencies;

    public GameStatsState(StateDependencies dependencies): base() {
        this.dependencies = dependencies;

        state.fps = 0;
    }
    
    public static float GetFps(GameStatsStateData state) {
        return state.fps;
    }


    public static void SetFps(BaseState<GameStatsStateData, GameStatsState> s, float args, Action c) { (s as GameStatsState).SFPS(c, args); }
    private void SFPS(Action complete, float args) {
        StateChange((GameStatsStateData state) => {
            state.fps = args;
        });
    }


}