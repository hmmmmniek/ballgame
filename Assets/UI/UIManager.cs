using System;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{


    public NetworkManager networkManager;

    private void OnEnable() {
        var stateDependencies = new StateDependencies(
            networkManager
        );
        StateManager.Init(stateDependencies);

        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        ViewManager.Init(root.Q<VisualElement>("Root"));

        ShowMenu();
    }

    public void ShowMenu() {
        var mainMenuController = ViewManager.instance.Open<MainMenuController>();
    }

}  


