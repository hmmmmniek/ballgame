using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using static CreateSessionController;

public enum Team {
    _, // Fusion doesnt seem to network first values of enums.. ?
    Blue,
    Red
}

public enum State {
    _, // Fusion doesnt seem to network first values of enums.. ?
    Started,
    ScoredReset,
    ScoredCountDown
}
public class MatchController : NetworkBehaviour {
    public static MatchController instance;

    public static async Task PlayerJoined(PlayerRef player, string hwid) {
        while(instance == null) {
            await Task.Delay(50);
        }
        if(instance.Runner.IsServer) {
            instance.HandlePlayerJoined(player, hwid);
        } else {
            instance.RPC_Joined(player, hwid);
        }
    }

    public BallController ballPrefab;
    public MapController mapPrefab;
    public PlayerController playerPrefab;
    public float resetPlayerVelocity = 3f;
    public float scoreCountDownTime = 5f;
    public float resetBallPlayerPushRadius = 5f;
    public float matchDurationSeconds = 600f;
    private Action unsubscribePlayers;


    [HideInInspector]public BallController ball;
    [HideInInspector]public MapController map;
    [HideInInspector]public MapInfo? mapInfo;
    private Player[] activePlayers;

    [HideInInspector][Networked(OnChanged = nameof(OnPlayersChanged))]
    public PlayerRefSet players { get; set; }
    public static void OnPlayersChanged(Changed<MatchController> changed) {
        changed.Behaviour.OnPlayersChanged();
    }
    private void OnPlayersChanged() {
        Player[] localPlayers = GameState.SelectOnce<Player[]>(GameState.GetPlayers);
        PlayerRef[] newJoinedPlayers = players.Where((p) =>
            !localPlayers.Any(lp => lp.playerRef.HasValue && lp.playerRef.Value.PlayerId == p.PlayerId)
        ).ToArray();
        Player[] disconnectedPlayers = localPlayers.Where((lp) =>
            !players.Any(p => lp.playerRef.HasValue && lp.playerRef.Value.PlayerId == p.PlayerId)
        ).ToArray();

        foreach (var playerRef in newJoinedPlayers) {
            GameState.Dispatch(GameState.AddPlayer, new Player(null, playerRef, null, null, null, Runner.LocalPlayer.PlayerId == playerRef.PlayerId), () => {});
        }
        foreach (var player in disconnectedPlayers) {
            GameState.Dispatch(GameState.RemovePlayer, player.hwid, () => {});
        }
    }

    [HideInInspector][Networked(OnChanged = nameof(OnStateChanged))]
    public State state { get; set; }
    public static void OnStateChanged(Changed<MatchController> changed) {
        changed.Behaviour.OnStateChanged();
    }
    private void OnStateChanged() {
       // GameState.Dispatch(GameState.SetState, state, () => {});
    }

    [HideInInspector][Networked(OnChanged = nameof(OnTeamBlueScoreChanged))]
    public int teamBlueScore { get; set; }
    public static void OnTeamBlueScoreChanged(Changed<MatchController> changed) {
        GameState.Dispatch(GameState.SetScore, (team: Team.Blue, score: changed.Behaviour.teamBlueScore), () => {});
    }
    [HideInInspector][Networked(OnChanged = nameof(OnTeamRedScoreChanged))]
    public int teamRedScore { get; set; }
    public static void OnTeamRedScoreChanged(Changed<MatchController> changed) {
        GameState.Dispatch(GameState.SetScore, (team: Team.Red, score: changed.Behaviour.teamRedScore), () => {});
    }

    [HideInInspector][Networked(OnChanged = nameof(OnCountDownStartChanged))]
    public float scoreCountDownStart { get; set; }
    public static void OnCountDownStartChanged(Changed<MatchController> changed) {
        GameState.Dispatch(GameState.SetScoredCountDownEnd, Time.time + changed.Behaviour.scoreCountDownTime, () => {});
    }

    [HideInInspector][Networked(OnChanged = nameof(OnMatchEndChanged))]
    public float matchEnd { get; set; }
    public static void OnMatchEndChanged(Changed<MatchController> changed) {
        changed.Behaviour.OnMatchEndChanged();
    }
    private void OnMatchEndChanged() {
        float duration = matchEnd - Runner.SimulationTime;
        GameState.Dispatch(GameState.SetMatchEnd, Time.time + duration, () => {});
    }

    [HideInInspector][Networked] public Team lastScored { get; set; }
    [HideInInspector][Networked] public float lastScoredTime { get; set; }

    public override void FixedUpdateNetwork() {
        if(!Object.HasStateAuthority) {
            
            if(ball == null) {
                ball = FindObjectOfType<BallController>();
            }

            if(map == null) {
                map = FindObjectOfType<MapController>();
            }
        }
        if(Object.HasStateAuthority && state == State.Started && mapInfo.HasValue) {
            Vector3 ballPos = ball.transform.position;
            
            if(
                (ballPos.z > ((mapInfo.Value.mapLength / 2) + ball.radius)) &&
                (ballPos.y > 0) &&
                (ballPos.y < (mapInfo.Value.mapGoalHeight)) &&
                (ballPos.x < (mapInfo.Value.mapGoalWidth / 2)) &&
                (ballPos.x > -(mapInfo.Value.mapGoalWidth / 2))
            ) {
                Scored(Team.Blue);
            }
            if(
                (ballPos.z < -((mapInfo.Value.mapLength / 2) + ball.radius)) &&
                (ballPos.y > 0) &&
                (ballPos.y < (mapInfo.Value.mapGoalHeight)) &&
                (ballPos.x < (mapInfo.Value.mapGoalWidth / 2)) &&
                (ballPos.x > -(mapInfo.Value.mapGoalWidth / 2))
            ) {
                Scored(Team.Red);
            }
        }
        if(Object.HasStateAuthority && (state == State.ScoredReset || state == State.ScoredCountDown) && mapInfo.HasValue) {
            bool playerPushed = false;
            foreach (var player in activePlayers) {
                if(playerPushed) {
                    continue;
                }
                playerPushed = !IsPlayerOnOwnHalf(player.playerController.transform.position, player.team.Value);
            }
          
            if(state != State.ScoredCountDown && !playerPushed) {
                state = State.ScoredCountDown;
                scoreCountDownStart = Runner.SimulationTime;
            }
        }
        if(Object.HasStateAuthority && state == State.ScoredCountDown && Runner.SimulationTime - scoreCountDownStart > scoreCountDownTime) {
            ball.EnablePhysics();
            state = State.Started;
            matchEnd = matchEnd + (Runner.SimulationTime - lastScoredTime);
        }

    }

    public void Scored(Team team) {
        switch(team) {
            case Team.Blue: {
                teamBlueScore = teamBlueScore + 1;
                break;
            }
            case Team.Red: {
                teamRedScore = teamRedScore + 1;
                break;
            }
        }
        ball.Reset();
        state = State.ScoredReset;
        lastScored = team;
        lastScoredTime = Runner.SimulationTime;
    }

    public override void Spawned() {
        base.Spawned();
        instance = this;

        if(map == null) {
            map = Runner.Spawn(mapPrefab, new Vector3(0, 0, 0), Quaternion.LookRotation(new Vector3(0, 0, 0)));
        }
        if(ball == null) {
            ball = Runner.Spawn(
                ballPrefab,
                new Vector3(0, 4, 0),
                Quaternion.LookRotation(new Vector3(0, 0, 0))
            );
        }

        if(Runner.SessionInfo.Properties.TryGetValue("mapSize", out var mapSize)) {
            MapGenerator gen = new MapGenerator();
            mapInfo = gen.GetMapInfo((MapSize)(int)mapSize);
        }

        if(Object.HasStateAuthority) {
            unsubscribePlayers = GameState.Select<Player[]>(GameState.GetPlayers, (statePlayers) => {
                HandlePlayersState(statePlayers);
                activePlayers = statePlayers.Where((p) => p.playerController != null && p.team.HasValue).ToArray();
            });
        }
        if(matchEnd == 0 && Object.HasStateAuthority) {
            matchEnd = Runner.SimulationTime + matchDurationSeconds;
            
            Scored(Team._);
        }

    }

    public void HandlePlayersState(Player[] statePlayers) {
        Player[] joinedPlayers = statePlayers.Where((p) =>
            p.playerController == null &&
            p.team.HasValue
        ).ToArray();
        
        Player[] switchedTeamPlayers = statePlayers.Where((p) =>
            p.playerController != null &&
            p.team.HasValue &&
            p.playerController.team != p.team.Value    
        ).ToArray();

        Player[] leftPlayers = statePlayers.Where((p) =>
            p.playerController != null &&
            !p.team.HasValue
        ).ToArray();
        
        Player[] migratedPlayers = statePlayers.Where((p) =>
            p.playerController != null &&
            p.team.HasValue &&
            p.hwid != null &&
            p.playerRef.HasValue &&
            p.playerController.inputAuthority == PlayerRef.None
        ).ToArray();

        foreach (var joinedPlayer in joinedPlayers) {

            PlayerController player = Runner.Spawn(
                prefab: playerPrefab,
                position: new Vector3(),
                rotation: Quaternion.identity,
                inputAuthority: joinedPlayer.playerRef.Value
            );
            player.inputAuthority = joinedPlayer.playerRef.Value;
            player.team = joinedPlayer.team.Value;

        }

        foreach (var switchedTeamPlayer in switchedTeamPlayers) {
            switchedTeamPlayer.playerController.team = switchedTeamPlayer.team.Value; 
        }
        foreach (var leftPlayer in leftPlayers) {
            Runner.Despawn(leftPlayer.playerController.GetComponent<NetworkObject>());
        }

        foreach (var migratedPlayer in migratedPlayers) {
            migratedPlayer.playerController.inputAuthority = migratedPlayer.playerRef.Value;        
        }
    }

    public void HandlePlayerJoined(PlayerRef playerRef, string hwid) {
        if(Object.HasStateAuthority) {
            IEnumerable<Player> localPlayers = GameState.SelectOnce(GameState.GetPlayers).Where(p => p.hwid == hwid);
            if(localPlayers.Count() > 0) {
                Player localPlayer = localPlayers.First();
                localPlayer.playerRef = playerRef;
                GameState.Dispatch(GameState.UpdatePlayer, (player: localPlayer, usePlayerRef: false), () => {});
            }
            PlayerRefSet newPlayers = players;
            newPlayers.Set(playerRef);
            players = newPlayers;

        }
    }

    public void HandlePlayerDisconnected(PlayerRef playerRef) {
        if(Object.HasStateAuthority) {
            Debug.Log("HandlePlayerDisconnected");
            Player player = GameState.SelectOnce<Player[]>(GameState.GetPlayers).Where(p => p.playerRef.HasValue && p.playerRef.Value.PlayerId == playerRef.PlayerId).First();
            Runner.Despawn(player.playerController.GetComponent<NetworkObject>());

            PlayerRefSet newPlayers = players;
            newPlayers.Clear(playerRef);
            players = newPlayers;
        }
    }
    public void HandlePlayerDisconnected(string hwid) {
        if(Object.HasStateAuthority) {
            Player player = GameState.SelectOnce<Player[]>(GameState.GetPlayers).Where(p => p.hwid == hwid).First();
            GameState.Dispatch(GameState.RemovePlayer, hwid, () => {});
            Runner.Despawn(player.playerController.GetComponent<NetworkObject>());
        }
    }

    public void ChangeTeam(PlayerRef playerRef, Team? team) {
        RPC_ChangeTeam(playerRef, team.HasValue ? (int)team : -1);
    }

    [Rpc(sources: RpcSources.All, targets: RpcTargets.All, InvokeLocal = true, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_ChangeTeam(PlayerRef playerRef, int team, RpcInfo info = default){
        if(info.Source == playerRef) {
            Player localPlayer = GameState.SelectOnce(GameState.GetPlayers).Where(p => p.playerRef.HasValue && p.playerRef.Value.PlayerId == playerRef.PlayerId).First();
            localPlayer.team = team == -1 ? null : (Team)team;
            GameState.Dispatch(GameState.UpdatePlayer, (player: localPlayer, usePlayerRef: true), () => {});
        }
    }


    [Rpc(sources: RpcSources.All, targets: RpcTargets.All, InvokeLocal = false, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_Joined(PlayerRef playerRef, string hwid, RpcInfo info = default){
        if(info.Source == playerRef && Runner.IsServer) {
            HandlePlayerJoined(playerRef, hwid);
        }
    }


    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);
        if(Object.HasStateAuthority) {
            unsubscribePlayers();

        }
    }

    public bool IsPlayerOnOwnHalf(Vector3 position, Team team) {
        Vector3 ballToPosition = new Vector3(position.x, 0, position.z) - new Vector3(ball.transform.position.x, 0, ball.transform.position.z);
        if(
            (
                (team == Team.Blue && position.z > 0) ||
                (team == Team.Red && position.z < 0)
            ) &&
            !(lastScored != team && ballToPosition.magnitude < mapInfo.Value.middleCircleRadius)
        ) {
            return false;
        }
        if(lastScored == team || lastScored == Team._) {
            if(ballToPosition.magnitude < mapInfo.Value.middleCircleRadius) {
                return false;
            }
        }
        return true;
    }
}
