using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using static CreateSessionController;

public class ScoreAndTimeController : Widget {
    public new const string ELEMENT_NAME = "ScoreAndTime";
    public new const string TEMPLATE_SELECTOR = "scoreAndTime.uxml";
    private VisualElement element;
    private Label blueScoreLabel;
    private Label redScoreLabel;
    private Label minutesLabel;
    private Label secondsLabel;

    private float matchEnd = Time.time;

    public ScoreAndTimeController(VisualElement element) {
        this.element = element;
        
        blueScoreLabel = element.Q<Label>("scoreAndTime__blue-score__score-label");
        redScoreLabel = element.Q<Label>("scoreAndTime__red-score__score-label");
        minutesLabel = element.Q<Label>("scoreAndTime__time__minutes-label");
        secondsLabel = element.Q<Label>("scoreAndTime__time__seconds-label");
        Watch(GameState.Select(GameState.GetBlueTeamScore, (score) => {
            blueScoreLabel.text = $"{score}";
        }));
        Watch(GameState.Select(GameState.GetRedTeamScore, (score) => {
            redScoreLabel.text = $"{score}";
        }));        
        Watch(GameState.Select(GameState.GetMatchEnd, (matchEndTime) => {
            matchEnd = matchEndTime;
        }));
       // GetScoredCountDownEnd
        UpdateTime();
    }

    private async void UpdateTime() {
        await Task.Delay(100);

        float remainingSeconds = matchEnd - Time.time;
        int seconds = (int)Math.Floor(remainingSeconds % 60f);
        int minutes = (int)((remainingSeconds - seconds) / 60f);
        minutesLabel.text = $"{(minutes < 10 ? "0" : "")}{minutes}";
        secondsLabel.text = $"{(seconds < 10 ? "0" : "")}{seconds}";

        UpdateTime();
    }
}

