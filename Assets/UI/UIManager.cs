using System;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public VisualTreeAsset mainMenuTemplate;
    public VisualTreeAsset lobbyTemplate;
    public VisualTreeAsset settingsTemplate;
    public VisualTreeAsset gameTemplate;

    public PlayerSessionManager sessionManager;

    private UIDocument uiDocument;

    private void OnEnable() {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        
        ViewManager.Init(root.Q<VisualElement>("Root"), new ControllerDependencies(
            mainMenuTemplate,
            lobbyTemplate,
            settingsTemplate,
            gameTemplate,
            sessionManager
        ));

        ShowMenu();

    }

    public void ShowMenu() {
        var mainMenuController = ViewManager.instance.Open<MainMenuController>();
    }

}  


