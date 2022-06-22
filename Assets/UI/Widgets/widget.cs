using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Widget: Template {
    public const string ELEMENT_NAME = "";
    public const string TEMPLATE_SELECTOR = "";

    private List<Action> subscriptions = new List<Action>();
    protected void Watch(Action subscription) {
        subscriptions.Add(subscription);
    }
    public virtual void OnDestroy() {
        foreach (var unsubscribe in subscriptions) {
            unsubscribe();
        }
        subscriptions.Clear();
    }
}