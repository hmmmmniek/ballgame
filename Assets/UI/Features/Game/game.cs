using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class GameController: Module {
    public new const string TEMPLATE_SELECTOR = "game.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        CursorController.ELEMENT_NAME,
        MiniMapController.ELEMENT_NAME,
        ScoreAndTimeController.ELEMENT_NAME
    };
    VisualElement boostIndicator;
    VisualElement chargeIndicator;

    
    Label rttLabel;

    public GameController(VisualElement element) {
        InputHandler.instance.StartGameInput();
        boostIndicator = element.Q<VisualElement>("game__boost__indicator");
        chargeIndicator = element.Q<VisualElement>("game__charge__indicator");

        rttLabel = element.Q<Label>("game_rtt__label");


        Watch(GameState.Select<float>(GameState.GetRemainingBoostPercentage, (boost) => {
            boostIndicator.style.width = new StyleLength(new Length(boost, LengthUnit.Percent));
        }));


        Watch(GameState.Select<float>(GameState.GetChargePercentage, (charge) => {
            chargeIndicator.style.width = new StyleLength(new Length(charge, LengthUnit.Percent));
            if(charge != 0) {
                ChargeLoop();
            }
        }));



          
        Watch(NetworkStatsState.Select<float>(NetworkStatsState.GetRtt, (rtt) => {
            rttLabel.text = $"{rtt}{(rtt%1==0 ? ".0" : "")} ms";
        }));
        

    }

    public async void ChargeLoop() {
        await Task.Delay(10);
        float charge = GameState.SelectOnce<float>(GameState.GetChargePercentage);
        chargeIndicator.style.width = new StyleLength(new Length(charge, LengthUnit.Percent));
        if(charge != 0) {
            ChargeLoop();
        }
    }

}
