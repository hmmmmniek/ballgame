using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : Module {
    public new const string TEMPLATE_SELECTOR = "mainMenu.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        ""
    };
    
    private Player localPlayer;


    public MainMenuController(VisualElement element) {


        Button settingsButton = element.Q<Button>("SettingsButton");
        settingsButton.clicked += GoToSettings;

        Button lobbyButton = element.Q<Button>("LobbyButton");
        lobbyButton.clicked += GoToLobby;

        Button leaveMatchButton = element.Q<Button>("LeaveMatchButton");
        leaveMatchButton.clicked += LeaveMatch;

        Button returnToMatchButton = element.Q<Button>("ReturnToMatchButton");
        returnToMatchButton.clicked += ReturnToMatch;

        Watch(NetworkState.Select<bool>(NetworkState.GetJoined, (joined) => {
            if(joined) {
                leaveMatchButton.RemoveFromClassList("hidden");
                returnToMatchButton.RemoveFromClassList("hidden");
                lobbyButton.AddToClassList("hidden");
            } else {
                leaveMatchButton.AddToClassList("hidden");
                returnToMatchButton.AddToClassList("hidden");
                lobbyButton.RemoveFromClassList("hidden");
            }
        }));
        
        Watch(GameState.Select<Player[]>(GameState.GetPlayers, (players) => {
            foreach (var player in players) {
                if(player.isLocal) {
                    localPlayer = player;
                }
            }
        }));

    }

    private void GoToSettings() {
        NotificationState.Dispatch(NotificationState.Notify, (NotificationUrgency.Info, "its working!"), () => { });

        ViewManager.instance.Open<SettingsController>();
    }
    private void GoToLobby() {
        ViewManager.instance.Open<LobbyController>();
    }
    private void ReturnToMatch() {
        if(localPlayer.team.HasValue) {
            ViewManager.instance.Open<GameController>();
        } else {
            ViewManager.instance.Open<TeamSelectController>();
        }
    }
    private void LeaveMatch() {
        NetworkState.Dispatch<object>(NetworkState.Leave, null, () => {});
    }
    
}
