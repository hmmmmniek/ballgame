using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

public enum Team {
    _, // Fusion doesnt seem to network first values of enums.. ?
    Blue,
    Red
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
    private Action unsubscribePlayers;

    [HideInInspector]public BallController ball;
    [HideInInspector]public MapController map;

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
    

    public override void FixedUpdateNetwork() {

    }

    public override void Spawned() {
        base.Spawned();
        instance = this;
        if(map == null) {
            map = Runner.Spawn(mapPrefab, new Vector3(0, 0, 0), Quaternion.LookRotation(new Vector3(0, 0, 0)));
        }
        if(ball == null) {
            ball = Runner.Spawn(ballPrefab, new Vector3(0, 4, 0), Quaternion.LookRotation(new Vector3(0, 0, 0)));
        }

        if(Object.HasStateAuthority) {

            unsubscribePlayers = GameState.Select<Player[]>(GameState.GetPlayers, (statePlayers) => {
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


            });
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
}
