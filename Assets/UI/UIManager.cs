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

    private void OnEnable() {
        var stateDependencies = new StateDependencies(
            sessionManager
        );
        StateManager.Init(stateDependencies);

        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        var featureTemplates = new FeatureTemplates(
            mainMenuTemplate,
            lobbyTemplate,
            settingsTemplate,
            gameTemplate
        );
        ViewManager.Init(root.Q<VisualElement>("Root"), featureTemplates);

        ShowMenu();
    }

    public void ShowMenu() {
        var mainMenuController = ViewManager.instance.Open<MainMenuController>();
    }

}  


