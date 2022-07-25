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
        ScoreAndTimeController.ELEMENT_NAME,
        BallReleaseCountDownController.ELEMENT_NAME,
        ScoreboardController.ELEMENT_NAME
    };
    VisualElement boostIndicator;
    VisualElement chargeIndicator;
    VisualElement scoreboardContainer;
    Label rttLabel;
    Label fpsLabel;

    public GameController(VisualElement element) {
        InputHandler.instance.StartGameInput();
        boostIndicator = element.Q<VisualElement>("game__boost__indicator");
        chargeIndicator = element.Q<VisualElement>("game__charge__indicator");

        rttLabel = element.Q<Label>("game_rtt__label");
        fpsLabel = element.Q<Label>("game_fps__label");


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
            float ms = (float)Math.Round(rtt/1000f, 1);
            rttLabel.text = $"{ms}{(ms%1==0 ? ".0" : "")} ms";
        }));
        
          
        Watch(GameStatsState.Select<float>(GameStatsState.GetFps, (fps) => {
            fpsLabel.text = $"{fps}{(fps%1==0 ? ".0" : "")} fps";
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
