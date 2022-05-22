using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyController: Module {
    public new readonly static string TEMPLATE_SELECTOR = "lobbyTemplate";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        BackToMainController.ELEMENT_NAME
    };
    private PlayerSessionManager sessionManager;

    public LobbyController(VisualElement element, ControllerDependencies dependencies) {
        sessionManager = dependencies.sessionManager;

        Label textLabel = element.Q<Label>("TextLabel");
        textLabel.text = "test!!!!";

        Button startButton = element.Q<Button>("StartButton");
        startButton.clicked += JoinMatch;
    }
    private async void JoinMatch() {
        await sessionManager.Join();
        ViewManager.instance.Open<GameController>();
    }
}
