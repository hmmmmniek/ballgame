using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class SessionListItemController : Widget {
    public new const string ELEMENT_NAME = "SessionListItem";
    public new const string TEMPLATE_SELECTOR = "sessionListItem.uxml";
    private VisualElement element;

    public SessionListItemController(VisualElement element) {
        this.element = element;
    }

    public void Init(SessionInfo session) {
        element.Q<Label>("session-name__label").text = session.Name;
        element.Q<Label>("session-location__label").text = session.Region;
        element.Q<Label>("session-players__max-label").text = $"{session.MaxPlayers}";
        element.Q<Label>("session-players__connected-label").text = $"{session.PlayerCount}";

    
    }

}
