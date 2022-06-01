using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class NotificationController : Widget {
    public new const string ELEMENT_NAME = "Notification";

    VisualElement container;
    VisualElement loadingBar;
    float millisLoadingStart;
    public NotificationController(VisualElement element) {
        container = element.Q<VisualElement>("notification__container");
        loadingBar = element.Q<VisualElement>("notification__container__loading");

        NotificationState.Select<(NotificationUrgency urgency, string text)?>(NotificationState.GetCurrentNotification, (notification) => {
            if(notification.HasValue) {
                Label textLabel = element.Q<Label>("notification__container__text-label");
                Label urgencyLabel = element.Q<Label>("notification__container__urgency-label");
                if(textLabel == null || urgencyLabel == null) {
                    return;
                }
                switch(notification.Value.urgency) {
                    case NotificationUrgency.Error: {
                        urgencyLabel.text = "Error";
                        break;
                    }
                    case NotificationUrgency.Info: {
                        urgencyLabel.text = "Info";
                        break;
                    }
                    case NotificationUrgency.Warning: {
                        urgencyLabel.text = "Warning";
                        break;
                    }
                }
                textLabel.text = notification.Value.text;
                Show(5000);
            }
        });
    }

    private async void Show(int millis) {
        container.AddToClassList("show");
        millisLoadingStart = Time.time * 1000;
        Loading(millis);
        await Task.Delay(millis);
        container.RemoveFromClassList("show");

    }

    private async void Loading(float millis) {
        float millisLoadingCurrent = Time.time * 1000;
        float millisElapsed = millisLoadingCurrent - millisLoadingStart;
        float percentage = millisElapsed / millis * 100;
        loadingBar.style.width = new StyleLength(new Length(percentage, LengthUnit.Percent));
        await Task.Delay(250);
        if(percentage < 99) {
            Loading(millis);
        }
    }

}
