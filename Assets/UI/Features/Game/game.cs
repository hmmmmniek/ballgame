using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameController: Module {
    public new const string TEMPLATE_SELECTOR = "game.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        
    };

    VisualElement boostIndicator;
    VisualElement chargeIndicator;
    VisualElement crossHairContainer;

    public GameController(VisualElement element) {
        InputHandler.instance.StartGameInput();
        boostIndicator = element.Q<VisualElement>("game__boost__indicator");
        chargeIndicator = element.Q<VisualElement>("game__charge__indicator");
        crossHairContainer = element.Q<VisualElement>("game__crosshair__container");

        Watch(GameState.Select<float>(GameState.GetRemainingBoostPercentage, (boost) => {
            boostIndicator.style.width = new StyleLength(new Length(boost, LengthUnit.Percent));
        }));

        Watch(GameState.Select<float>(GameState.GetChargePercentage, (charge) => {
            chargeIndicator.style.width = new StyleLength(new Length(charge, LengthUnit.Percent));
        }));

        Watch(GameState.Select<bool>(GameState.GetCarryingBall, (carrying) => {
            if(carrying) {
                crossHairContainer.RemoveFromClassList("empty");
            } else {
                crossHairContainer.AddToClassList("empty");
            }
        }));

    }
}
