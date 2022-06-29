using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class InGameMenuController : Module {
    public new const string TEMPLATE_SELECTOR = "inGameMenu.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        ""
    };
    
    private Player localPlayer;


    public InGameMenuController(VisualElement element) {


        Button settingsButton = element.Q<Button>("SettingsButton");
        settingsButton.clicked += GoToSettings;

        Button leaveMatchButton = element.Q<Button>("LeaveMatchButton");
        leaveMatchButton.clicked += LeaveMatch;

        Button returnToMatchButton = element.Q<Button>("ReturnToMatchButton");
        returnToMatchButton.clicked += ReturnToMatch;

        Button switchTeamButton = element.Q<Button>("SwitchTeamButton");
        switchTeamButton.clicked += SwitchTeam;
     
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
            ViewManager.instance.Open<InGameMenuController>();
        };
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
        ViewManager.instance.Open<MainMenuController>();

    }
    private void SwitchTeam() {
        localPlayer.team = null;
        GameState.Dispatch(GameState.UpdatePlayer, localPlayer, () => {});
        ViewManager.instance.Open<TeamSelectController>();

    }
}
