using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public struct Player {
    public PlayerRef? playerRef;
    public BallGunController ballGunController;
    public PlayerController playerController;
    public Team? team;
    public bool isLocal;
    public string hwid;

    public Player(
        string hwid,
        PlayerRef? playerRef,
        BallGunController ballGunController,
        PlayerController playerController,
        Team? team,
        bool isLocal
    ) {
        this.hwid = hwid;
        this.playerRef = playerRef;
        this.ballGunController = ballGunController;
        this.playerController = playerController;
        this.team = team;
        this.isLocal = isLocal;
    }

}


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

    public Player[] players = {};
    public BallController ball;

    public int teamBlueScore;
    public int teamRedScore;
    public float scoredCountDownEnd;
    public float matchEnd;

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

    public static float GetScoredCountDownEnd(GameStateData state) {
        return state.scoredCountDownEnd;
    }

    public static int GetRedTeamScore(GameStateData state) {
        return state.teamRedScore;
    }

    public static int GetBlueTeamScore(GameStateData state) {
        return state.teamBlueScore;
    }


    public static Vector2 GetSpinInput(GameStateData state) {
        return state.spinInput;
    }

    public static Player[] GetPlayers(GameStateData state) {
        return state.players;
    }
    
    public static BallController GetBall(GameStateData state) {
        return state.ball;
    }
    
    public static float GetMatchEnd(GameStateData state) {
        return state.matchEnd;
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

    public static void AddPlayer(BaseState<GameStateData, GameState> s, Player args, Action c) { (s as GameState).AP(c, args); }
    private void AP(Action complete, Player args) {
        StateChange((GameStateData state) => {
            Player[] newPlayers = {args};
            state.players = newPlayers.Concat(state.players).ToArray();
            PrintPlayers(state.players);
        });
    }

    public static void RemovePlayer(BaseState<GameStateData, GameState> s, string args, Action c) { (s as GameState).RP(c, args); }
    private void RP(Action complete, string args) {
        StateChange((GameStateData state) => {
            state.players = state.players.Where((item) => item.hwid != args).ToArray();
            PrintPlayers(state.players);
        });
    }

    public static void UpdatePlayer(BaseState<GameStateData, GameState> s, (Player player, bool usePlayerRef) args, Action c) { (s as GameState).UP(c, args); }
    private void UP(Action complete, (Player player, bool usePlayerRef) args) {
        StateChange((GameStateData state) => {
            Player[] newPlayers = {args.player};
            state.players = args.usePlayerRef ? 
                newPlayers.Concat(state.players.Where((item) =>
                    (item.playerRef.HasValue && item.playerRef.Value.PlayerId != args.player.playerRef.Value.PlayerId)
                ).ToArray()).ToArray() :
                newPlayers.Concat(state.players.Where((item) =>
                    (item.hwid != args.player.hwid)
                ).ToArray()).ToArray();
            PrintPlayers(state.players);
        });
    }

    private void PrintPlayers(Player[] players) {
        
        //Debug.Log("-----");
        //foreach (var player in players) {
        //    Debug.Log($"PlayerID: {(player.playerRef.HasValue ? player.playerRef.Value.PlayerId : "?")} Team: {(player.team == Team.Blue ? "Blue" : (player.team == Team.Red ? "Red" : "None"))} Is local: {(player.isLocal ? "true" : "false") } Has playercontroller: {(player.playerController == null ? "false" : "true")} HWID: {player.hwid}");
        //}
    }


    public static void SetScore(BaseState<GameStateData, GameState> s, (Team team, int score) args, Action c) { (s as GameState).SS(c, args); }
    private void SS(Action complete, (Team team, int score) args) {
        StateChange((GameStateData state) => {
            switch(args.team) {
                case Team.Blue: {
                    state.teamBlueScore = args.score;
                    break;
                }
                case Team.Red: {
                    state.teamRedScore = args.score;
                    break;
                }
            }
        });
    }

    public static void SetScoredCountDownEnd(BaseState<GameStateData, GameState> s, float args, Action c) { (s as GameState).SSCDE(c, args); }
    private void SSCDE(Action complete, float args) {
        StateChange((GameStateData state) => {
            state.scoredCountDownEnd = args;
        });
    }

    public static void SetMatchEnd(BaseState<GameStateData, GameState> s, float args, Action c) { (s as GameState).SME(c, args); }
    private void SME(Action complete, float args) {
        StateChange((GameStateData state) => {
            state.matchEnd = args;
        });
    }



    public static void ClearPlayers(BaseState<GameStateData, GameState> s, object args, Action c) { (s as GameState).CP(c); }
    private void CP(Action complete) {
        StateChange((GameStateData state) => {
            Player[] newPlayers = {};
            state.players = newPlayers;
        });
    }

    public static void SetBall(BaseState<GameStateData, GameState> s, BallController args, Action c) { (s as GameState).SB(c, args); }
    private void SB(Action complete, BallController args) {
        StateChange((GameStateData state) => {
            state.ball = args;
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