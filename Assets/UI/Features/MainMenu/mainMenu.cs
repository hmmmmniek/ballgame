using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : Module {
    public new readonly static string TEMPLATE_SELECTOR = "mainMenuTemplate";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        ""
    };
    private PlayerSessionManager sessionManager;
    
    public MainMenuController(VisualElement element, ControllerDependencies dependencies) {

        sessionManager = dependencies.sessionManager;
        
        Button settingsButton = element.Q<Button>("SettingsButton");
        settingsButton.clicked += GoToSettings;

        Button lobbyButton = element.Q<Button>("LobbyButton");
        lobbyButton.clicked += GoToLobby;

        Button leaveMatchButton = element.Q<Button>("LeaveMatchButton");
        leaveMatchButton.clicked += LeaveMatch;

        Button returnToMatchButton = element.Q<Button>("ReturnToMatchButton");
        returnToMatchButton.clicked += ReturnToMatch;

        if(sessionManager.joined) {
            lobbyButton.AddToClassList("hidden");
        } else {
            leaveMatchButton.AddToClassList("hidden");
            returnToMatchButton.AddToClassList("hidden");
        }


    }

    private void GoToSettings() {
        ViewManager.instance.Open<SettingsController>();
    }
    private void GoToLobby() {
        ViewManager.instance.Open<LobbyController>();
    }
    private void ReturnToMatch() {
        ViewManager.instance.Open<GameController>();
    }
    private async void LeaveMatch() {
        await sessionManager.Leave();
    }
}
