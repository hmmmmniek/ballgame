using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LoginController : Module {
    public new const string TEMPLATE_SELECTOR = "login.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        ""
    };
    private string playerName;

    public LoginController(VisualElement element) {
        Button startButton = element.Q<Button>("StartButton");
        startButton.clicked += GoToMenu;




        TextField playerNameInput = element.Q<TextField>("player-name-input__textfield");
        playerNameInput.RegisterValueChangedCallback((change) => {
            playerName = change.newValue;
            UserState.Dispatch(UserState.SetPlayerName, playerName, () => {});
        });

        Button playerNameClearButton = element.Q<Button>("player-name-input__clear");
        playerNameClearButton.clicked += () => {
            playerNameInput.value = "";  
        };

        playerNameInput.value = UserState.SelectOnce(UserState.GetPlayerName);
        playerName = playerNameInput.value;

    }

    private void GoToMenu() {
        ViewManager.instance.Open<MainMenuController>();
    }


}


       
