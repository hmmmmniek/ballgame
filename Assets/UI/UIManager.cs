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
    public VisualTreeAsset sessionListItem;

    public NetworkManager networkManager;

    private void OnEnable() {
        var stateDependencies = new StateDependencies(
            networkManager
        );
        StateManager.Init(stateDependencies);

        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        var featureTemplates = new FeatureTemplates(
            mainMenuTemplate,
            lobbyTemplate,
            settingsTemplate,
            gameTemplate,
            sessionListItem
        );
        ViewManager.Init(root.Q<VisualElement>("Root"), featureTemplates);

        ShowMenu();
    }

    public void ShowMenu() {
        var mainMenuController = ViewManager.instance.Open<MainMenuController>();
    }

}  


