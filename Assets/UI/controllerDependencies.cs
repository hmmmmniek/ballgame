using Fusion;
using UnityEngine.UIElements;

public class FeatureTemplates {
    public VisualTreeAsset mainMenuTemplate;
    public VisualTreeAsset lobbyTemplate;
    public VisualTreeAsset settingsTemplate;
    public VisualTreeAsset gameTemplate;

    public FeatureTemplates(
        VisualTreeAsset mainMenuTemplate,
        VisualTreeAsset lobbyTemplate,
        VisualTreeAsset settingsTemplate,
        VisualTreeAsset gameTemplate
    ) {
        this.mainMenuTemplate = mainMenuTemplate;
        this.lobbyTemplate = lobbyTemplate;
        this.settingsTemplate = settingsTemplate;
        this.gameTemplate = gameTemplate;
    }
}
