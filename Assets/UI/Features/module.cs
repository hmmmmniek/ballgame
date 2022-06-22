using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Module: Template {
    public const string TEMPLATE_SELECTOR = "";
    public readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] { };
    
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