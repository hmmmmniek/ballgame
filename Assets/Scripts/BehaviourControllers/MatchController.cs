using UnityEngine;
using Fusion;
using System.Collections.Generic;
using System;
using System.Linq;

public enum Team {
    Blue,
    Red
}
public class MatchController : NetworkBehaviour {

    public BallController ballPrefab;
    public MapController mapPrefab;
    public PlayerController playerPrefab;
    private Action unsubscribePlayers;

    [HideInInspector][Networked(OnChanged = nameof(OnPlayersChanged))]
    public PlayerRefSet players { get; set; }
    public static void OnPlayersChanged(Changed<MatchController> changed) {
        changed.Behaviour.OnPlayersChanged();
    }
    private void OnPlayersChanged() {
        Player[] localPlayers = GameState.SelectOnce<Player[]>(GameState.GetPlayers);
        PlayerRef[] newJoinedPlayers = players.Where((p) =>
            !localPlayers.Any(lp => lp.playerRef.PlayerId == p.PlayerId)
        ).ToArray();
        Player[] disconnectedPlayers = localPlayers.Where((lp) =>
            !players.Any(p => lp.playerRef.PlayerId == p.PlayerId)
        ).ToArray();

        foreach (var playerRef in newJoinedPlayers) {
            GameState.Dispatch(GameState.AddPlayer, new Player(playerRef, null, null, null, Runner.LocalPlayer.PlayerId == playerRef.PlayerId), () => {});
        }
        foreach (var player in disconnectedPlayers) {
            GameState.Dispatch(GameState.RemovePlayer, player.playerRef, () => {});
        }
    }
    

    public override void FixedUpdateNetwork() {

    }
    public override void Spawned() {
        base.Spawned();
        MapController map = Runner.Spawn(mapPrefab, new Vector3(0, 0, 0), Quaternion.LookRotation(new Vector3(0, 0, 0)));
        BallController ball = Runner.Spawn(ballPrefab, new Vector3(0, 4, 0), Quaternion.LookRotation(new Vector3(0, 0, 0)));


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

                foreach (var joinedPlayer in joinedPlayers) {
                    PlayerController player = Runner.Spawn(playerPrefab, new Vector3(), Quaternion.identity, joinedPlayer.playerRef, (r, n) => {
                        n.GetComponent<PlayerController>().team = joinedPlayer.team.Value;
                    });
                }

                foreach (var switchedTeamPlayer in switchedTeamPlayers) {
                    switchedTeamPlayer.playerController.team = switchedTeamPlayer.team.Value; 
                }

                foreach (var leftPlayer in leftPlayers) {
                    Runner.Despawn(leftPlayer.playerController.GetComponent<NetworkObject>());
                    Player updatedPlayer = new Player(
                        leftPlayer.playerRef,
                        null,
                        null,
                        null,
                        leftPlayer.isLocal
                    );
                    GameState.Dispatch<Player>(GameState.UpdatePlayer, updatedPlayer, () => {});
                }

            });
        } 
        

    }

    public void PlayerJoined(PlayerRef playerRef) {
        PlayerRefSet newPlayers = players;
        newPlayers.Set(playerRef);
        players = newPlayers;
    }

    public void PlayerDisconnected(PlayerRef playerRef) {
        PlayerRefSet newPlayers = players;
        newPlayers.Clear(playerRef);
        players = newPlayers;
    }


    public override void Despawned(NetworkRunner runner, bool hasState) {
        base.Despawned(runner, hasState);

        unsubscribePlayers();
    }
}
