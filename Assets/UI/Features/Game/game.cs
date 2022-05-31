using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameController: Module {
    public new const string TEMPLATE_SELECTOR = "game.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        
    };


    public GameController(VisualElement element) {
        InputHandler.instance.StartGameInput();
    }

}
