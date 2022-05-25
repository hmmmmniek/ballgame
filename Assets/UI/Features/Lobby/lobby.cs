using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyController: Module {
    public new readonly static string TEMPLATE_SELECTOR = "lobbyTemplate";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        BackToMainController.ELEMENT_NAME
    };

    public LobbyController(VisualElement element) {

        Label textLabel = element.Q<Label>("TextLabel");
        textLabel.text = "test!!!!";

        Button startButton = element.Q<Button>("StartButton");
        startButton.clicked += JoinMatch;
    }
    private async void JoinMatch() {
        await StateManager.instance.networkState.Join();
        ViewManager.instance.Open<GameController>();
    }
}
