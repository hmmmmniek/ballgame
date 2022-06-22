using System.Collections.Generic;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class CursorController : Widget {
    public new const string ELEMENT_NAME = "Cursor";
    public new const string TEMPLATE_SELECTOR = "cursor.uxml";
    private VisualElement element;

    public CursorController(VisualElement element) {
        this.element = element;
    }

    

}
