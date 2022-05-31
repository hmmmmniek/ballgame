
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class ViewManager {
    private static ViewManager _instance = null;
    public static ViewManager instance {
        get {
            return _instance;
        }
    }
    public static void Init(VisualElement root) {
        _instance = new ViewManager(root);
    }

    private VisualElement root;
    private VisualElement currentModule;

    private ViewManager(VisualElement root) {
        this.root = root;
    }

    public T Open<T>() where T: Module {
        if(currentModule != null) {
            root.Remove(currentModule);
        }
        var template = GetTemplate<T>();
        var element = template.Instantiate();

        var controller = Activator.CreateInstance(typeof(T), new object[] { element }) as T;
        element.userData = controller;
        root.Add(element);
        currentModule = element;

        InitializeWidgetsOfModule<T>();

        return controller;
    }

    public VisualTreeAsset GetWidgetTemplate<T>() where T: Widget {
        return GetTemplate<T>();
    }

    private VisualTreeAsset GetTemplate<T>() where T: Template {
        var templateSelector = typeof(T).GetField("TEMPLATE_SELECTOR").GetValue(null) as string;
        VisualTreeAsset template = Resources.Load<VisualTreeAsset>(Path.GetFileNameWithoutExtension(templateSelector));
        return template;
    }

    private void InitializeWidgetsOfModule<T>() where T: Module {
        var elementNames = typeof(T).GetField("WIDGET_ELEMENT_NAMES").GetValue(null) as IEnumerable<string>;
        foreach (var elementName in elementNames) {
            switch (elementName){
                case BackButtonController.ELEMENT_NAME: {
                    InitializeWidgetsOfType<BackButtonController>();
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

