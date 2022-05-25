using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BackToMainController : Widget {
    public new const string ELEMENT_NAME = "BackToMain";


    public BackToMainController(VisualElement element) {
        Button backButton = element.Q<Button>("BackButton");
        backButton.clicked += GoToMainMenu;
    }

    private void GoToMainMenu() {
        ViewManager.instance.Open<MainMenuController>();
    }
}
