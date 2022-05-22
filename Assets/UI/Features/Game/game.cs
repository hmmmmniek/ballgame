using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameController: Module {
    public new readonly static string TEMPLATE_SELECTOR = "gameTemplate";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        
    };


    public GameController(VisualElement element, ControllerDependencies dependencies) {
        InputHandler.instance.StartGameInput();
    }

}
