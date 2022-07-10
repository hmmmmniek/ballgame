using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerListItemController : Widget {
    public new const string ELEMENT_NAME = "PlayerListItem";
    public new const string TEMPLATE_SELECTOR = "playerListItem.uxml";
    private VisualElement element;

    public PlayerListItemController(VisualElement element) {
        this.element = element;
    }

    public void Init(Player player) {
        
        element.Q<Label>("player-name__label").text = player.name;

    }

}
