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

    public bool shielding;
    public bool attracting;
    public bool inputtingRoll;
    public bool inputtingSpin;
    
    public float rollInput;
    public Vector2 spinInput;

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

    public static bool GetInputtingSpin(GameStateData state) {
        return state.inputtingSpin;
    }

    public static bool GetInputtingRoll(GameStateData state) {
        return state.inputtingRoll;
    }

    public static bool GetAttracting(GameStateData state) {
        return state.attracting;
    }

    public static bool GetShielding(GameStateData state) {
        return state.shielding;
    }

    public static float GetRollInput(GameStateData state) {
        return state.rollInput;
    }


    public static Vector2 GetSpinInput(GameStateData state) {
        return state.spinInput;
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


    public static void SetShielding(BaseState<GameStateData, GameState> s, bool args, Action c) { (s as GameState).SS(c, args); }
    private void SS(Action complete, bool args) {
        StateChange((GameStateData state) => {
            state.shielding = args;
        });
    }

    public static void SetInputtingRoll(BaseState<GameStateData, GameState> s, bool args, Action c) { (s as GameState).SIR(c, args); }
    private void SIR(Action complete, bool args) {
        StateChange((GameStateData state) => {
            state.inputtingRoll = args;
        });
    }

    public static void SetInputtingSpin(BaseState<GameStateData, GameState> s, bool args, Action c) { (s as GameState).SIS(c, args); }
    private void SIS(Action complete, bool args) {
        StateChange((GameStateData state) => {
            state.inputtingSpin = args;
        });
    }

    public static void SetAttracting(BaseState<GameStateData, GameState> s, bool args, Action c) { (s as GameState).SA(c, args); }
    private void SA(Action complete, bool args) {
        StateChange((GameStateData state) => {
            state.attracting = args;
        });
    }

    public static void SetRollInput(BaseState<GameStateData, GameState> s, float args, Action c) { (s as GameState).SRI(c, args); }
    private void SRI(Action complete, float args) {
        StateChange((GameStateData state) => {
            state.rollInput = args;
        });
    }

    public static void SetSpinInput(BaseState<GameStateData, GameState> s, Vector2 args, Action c) { (s as GameState).SSI(c, args); }
    private void SSI(Action complete, Vector2 args) {
        StateChange((GameStateData state) => {
            state.spinInput = args;
        });
    }
}