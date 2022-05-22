using Fusion;
using UnityEngine.UIElements;

public class ControllerDependencies {
    public VisualTreeAsset mainMenuTemplate;
    public VisualTreeAsset lobbyTemplate;
    public VisualTreeAsset settingsTemplate;
    public VisualTreeAsset gameTemplate;

    public PlayerSessionManager sessionManager;
    public ControllerDependencies(
        VisualTreeAsset mainMenuTemplate,
        VisualTreeAsset lobbyTemplate,
        VisualTreeAsset settingsTemplate,
        VisualTreeAsset gameTemplate,
        PlayerSessionManager sessionManager
    ) {
        this.mainMenuTemplate = mainMenuTemplate;
        this.lobbyTemplate = lobbyTemplate;
        this.settingsTemplate = settingsTemplate;
        this.gameTemplate = gameTemplate;
        this.sessionManager = sessionManager;
    }
}
