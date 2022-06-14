using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class GameStateData: StateData {
    public float remainingBoostPercentage;
    public float chargeTime;
    public float chargeStart;
    public bool carryingBall;
    public (BallGunController ballGunController, PlayerController playerController)[] players = {};
}

public class GameState: BaseState<GameStateData, GameState> {
    public const string SELECTOR = "Game";

    private StateDependencies dependencies;

    public GameState(StateDependencies dependencies): base() {
        this.dependencies = dependencies;
        state.remainingBoostPercentage = 100f;
        state.chargeStart = -1;
    }
    
    public static float GetRemainingBoostPercentage(GameStateData state) {
        return state.remainingBoostPercentage;
    }

    public static void SetRemainingBoostPercentage(BaseState<GameStateData, GameState> s, float args, Action c) { (s as GameState).SRBP(c, args); }
    private void SRBP(Action complete, float args) {
        StateChange((GameStateData state) => {
            state.remainingBoostPercentage = args;
        });
    }

    public static float GetChargePercentage(GameStateData state) {
        if(!state.carryingBall || state.chargeStart == -1) {
            return 0;
        } else {
            return Math.Clamp((Time.time - state.chargeStart) / state.chargeTime * 100, 1, 100);
        }
    }

    public static bool GetCarryingBall(GameStateData state) {
        return state.carryingBall;
    }




    public static (BallGunController ballGunController, PlayerController playerController)[] GetPlayers(GameStateData state) {
        return state.players;
    }

    public static void SetIsCharging(BaseState<GameStateData, GameState> s, bool args, Action c) { (s as GameState).SICH(c, args); }
    private void SICH(Action complete, bool args) {
        StateChange((GameStateData state) => {
            if(args) {
                state.chargeStart = Time.time;
            } else {
                state.chargeStart = -1;
            }
            
        });
    }

    public static void SetChargeTime(BaseState<GameStateData, GameState> s, float args, Action c) { (s as GameState).SCT(c, args); }
    private void SCT(Action complete, float args) {
        StateChange((GameStateData state) => {
            state.chargeTime = args;
        });
    }

    public static void AddPlayer(BaseState<GameStateData, GameState> s, (BallGunController ballGunController, PlayerController playerController) args, Action c) { (s as GameState).AP(c, args); }
    private void AP(Action complete, (BallGunController ballGunController, PlayerController playerController) args) {
        StateChange((GameStateData state) => {
            (BallGunController ballGunController, PlayerController playerController)[] newPlayers = {args};
            state.players = newPlayers.Concat(state.players).ToArray();
        });
    }


    public static void RemovePlayer(BaseState<GameStateData, GameState> s, (BallGunController ballGunController, PlayerController playerController) args, Action c) { (s as GameState).RP(c, args); }
    private void RP(Action complete, (BallGunController ballGunController, PlayerController playerController) args) {
        StateChange((GameStateData state) => {
            state.players = state.players.Where((item) => item.playerController.Id != args.playerController.Id).ToArray();
        });
    }

    public static void ClearPlayers(BaseState<GameStateData, GameState> s, object args, Action c) { (s as GameState).CP(c); }
    private void CP(Action complete) {
        StateChange((GameStateData state) => {
            (BallGunController ballGunController, PlayerController playerController)[] newPlayers = {};
            state.players = newPlayers;
        });
    }


    public static void SetCarryingBall(BaseState<GameStateData, GameState> s, bool args, Action c) { (s as GameState).SCB(c, args); }
    private void SCB(Action complete, bool args) {
        StateChange((GameStateData state) => {
            state.carryingBall = args;
        });
    }

}