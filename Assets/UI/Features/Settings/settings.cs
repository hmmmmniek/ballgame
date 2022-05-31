using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsController : Module {
    public new const string TEMPLATE_SELECTOR = "settings.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        BackButtonController.ELEMENT_NAME
    };

    public SettingsController(VisualElement element) {

        Label textLabel = element.Q<Label>("TextLabel");
        textLabel.text = "test!!!!";

        Button backButton = element.Q<Button>("BackButton");
        backButton.clicked += GoToMainMenu;
    }


    private void GoToMainMenu() {
        ViewManager.instance.Open<MainMenuController>();
    }
}
