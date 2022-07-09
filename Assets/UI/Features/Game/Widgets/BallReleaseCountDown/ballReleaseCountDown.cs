using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class BallReleaseCountDownController : Widget {
    public new const string ELEMENT_NAME = "BallReleaseCountDown";
    public new const string TEMPLATE_SELECTOR = "ballReleaseCountDown.uxml";
    private VisualElement element;
    private VisualElement progressBar;
    private VisualElement wrapper;

    private float countDownEnd = Time.time;
    private float countDownDuration = 0;
    public BallReleaseCountDownController(VisualElement element) {
        this.element = element;
        wrapper = element.Q<VisualElement>("ballReleaseCountDown__container");
        progressBar = element.Q<VisualElement>("ballReleaseCountDown__progress");
      
        Watch(GameState.Select(GameState.GetScoredCountDownEnd, (end) => {
            countDownEnd = end;
            countDownDuration = countDownEnd - Time.time;
            StyleLength amount = new StyleLength(new Length(0, LengthUnit.Percent));
            progressBar.style.width = amount;
            if(countDownEnd > Time.time) {
                wrapper.RemoveFromClassList("hidden");
                UpdateTime();
            }
        }));
    }

    private async void UpdateTime() {
        await Task.Delay(10);

        float remainingSeconds = countDownEnd - Time.time;
        float progressPercentage = (1f - remainingSeconds / countDownDuration) * 100;
        StyleLength amount = new StyleLength(new Length(progressPercentage, LengthUnit.Percent));
        progressBar.style.width = amount;
        if(progressPercentage < 100) {
            UpdateTime();
        } else {
            wrapper.AddToClassList("hidden");
        }
        
    }
}

