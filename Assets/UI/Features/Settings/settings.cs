using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsController : Module {
    public new readonly static string TEMPLATE_SELECTOR = "settingsTemplate";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        BackToMainController.ELEMENT_NAME
    };

    public SettingsController(VisualElement element) {

        Label textLabel = element.Q<Label>("TextLabel");
        textLabel.text = "test!!!!";
    }
}
