using System;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{

    public NetworkManager networkManager;
    public PlayerInput playerInput;

    private void OnEnable() {
        var stateDependencies = new StateDependencies(
            networkManager,
            playerInput
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


