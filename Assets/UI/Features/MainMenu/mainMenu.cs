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

        SettingsController controller = ViewManager.instance.Open<SettingsController>();
        controller.backAction = () => {
            ViewManager.instance.Open<MainMenuController>();
        };
    }
    private void GoToLobby() {
        ViewManager.instance.Open<LobbyController>();
    }

    
}
