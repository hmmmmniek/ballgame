
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ViewManager {
    private static ViewManager _instance = null;
    public static ViewManager instance {
        get {
            return _instance;
        }
    }
    public static void Init(VisualElement root, FeatureTemplates templates) {
        _instance = new ViewManager(root, templates);
    }

    private VisualElement root;
    private VisualElement currentModule;
    private FeatureTemplates templates;

    private ViewManager(VisualElement root, FeatureTemplates templates) {
        this.root = root;
        this.templates = templates;
    }

    public T Open<T>() where T: Module {
        if(currentModule != null) {
            root.Remove(currentModule);
        }
        var templateSelector = typeof(T).GetField("TEMPLATE_SELECTOR").GetValue(null) as string;
        var template = templates.GetType().GetField(templateSelector).GetValue(templates) as VisualTreeAsset;
        var element = template.Instantiate();
        var controller = Activator.CreateInstance(typeof(T), new object[] { element }) as T;
        element.userData = controller;
        root.Add(element);
        currentModule = element;

        var widgetElementNames = typeof(T).GetField("WIDGET_ELEMENT_NAMES").GetValue(null) as IEnumerable<string>;
        InitializeWidgets(widgetElementNames);

        return controller;
    }

    private void InitializeWidgets(IEnumerable<string> elementNames) {
        foreach (var elementName in elementNames) {
            switch (elementName){
                case BackToMainController.ELEMENT_NAME: {
                    InitializeWidgetsOfType<BackToMainController>();
                    break;
                }
            }
        }
    }

    private List<T> InitializeWidgetsOfType<T>() where T: Widget {
        var elementName = typeof(T).GetField("ELEMENT_NAME").GetValue(null) as string;
        var widgetElements = new List<VisualElement>();
        root.Query(elementName).Descendents<VisualElement>().Build().ToList(widgetElements);
        
        var controllers = new List<T>();
        foreach (var widgetElement in widgetElements) {
            var controller = Activator.CreateInstance(typeof(T), new object[] { widgetElement }) as T;
            widgetElement.userData = controller;
            controllers.Add(controller);
        }

        return controllers;
    }
}

