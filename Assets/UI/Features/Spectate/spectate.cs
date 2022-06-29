using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class SpectateController : Module {
    public new const string TEMPLATE_SELECTOR = "spectate.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
    };

    private VisualElement element;
   

    public SpectateController(VisualElement element) {
        this.element = element;
        
        Button stopSpectateButton = element.Q<Button>("stop-spectate__button");
        stopSpectateButton.clicked += StopSpectate;
        

    }

    private void StopSpectate() {
        ViewManager.instance.Open<TeamSelectController>();

    }
}