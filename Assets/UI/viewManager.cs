
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
        InitializeWidgetsOfType<NotificationController>();
    }

    public T Open<T>() where T: Module {
        if(currentModule != null) {
            root.Remove(currentModule);
        }
        var template = GetTemplate<T>();
        var element = template.Instantiate();
        root.Add(element);

        var controller = Activator.CreateInstance(typeof(T), new object[] { element }) as T;
        var widgets = InitializeWidgetsOfModule<T>();
        var userData = new Dictionary<string, object>();
        userData.Add("controller", controller);
        userData.Add("type", "module");
        userData.Add("widgets", widgets);
        element.userData = userData;

        element.RegisterCallback<DetachFromPanelEvent>((e) => {
            foreach (var widget in widgets) {
                (widget as Widget).OnDestroy();
            }
            (controller as Module).OnDestroy();
        });
        currentModule = element;


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

    private List<Widget> InitializeWidgetsOfModule<T>() where T: Module {
        var elementNames = typeof(T).GetField("WIDGET_ELEMENT_NAMES").GetValue(null) as IEnumerable<string>;
        var widgets = new List<Widget>();
        foreach (var elementName in elementNames) {
            switch (elementName){
                case BackButtonController.ELEMENT_NAME: {
                    widgets.AddRange(InitializeWidgetsOfType<BackButtonController>());
                    break;
                }
                case CursorController.ELEMENT_NAME: {
                    widgets.AddRange(InitializeWidgetsOfType<CursorController>());
                    break;
                }
                case MiniMapController.ELEMENT_NAME: {
                    widgets.AddRange(InitializeWidgetsOfType<MiniMapController>());
                    break;
                }
                case ScoreAndTimeController.ELEMENT_NAME: {
                    widgets.AddRange(InitializeWidgetsOfType<ScoreAndTimeController>());
                    break;
                }
                case BallReleaseCountDownController.ELEMENT_NAME: {
                    widgets.AddRange(InitializeWidgetsOfType<BallReleaseCountDownController>());
                    break;
                }
                case ScoreboardController.ELEMENT_NAME: {
                    widgets.AddRange(InitializeWidgetsOfType<ScoreboardController>());
                    break;
                }
            }
        }
        return widgets;
    }


    private List<T> InitializeWidgetsOfType<T>() where T: Widget {
        var elementName = typeof(T).GetField("ELEMENT_NAME").GetValue(null) as string;
        var widgetElements = new List<VisualElement>();
        root.Query(elementName).Build().ToList(widgetElements);
        var controllers = new List<T>();
        foreach (var widgetElement in widgetElements) {
            var controller = Activator.CreateInstance(typeof(T), new object[] { widgetElement }) as T;
            var userData = new Dictionary<string, object>();
            userData.Add("controller", controller);
            userData.Add("type", "widget");
            widgetElement.userData = userData;
            controllers.Add(controller);
        }

        return controllers;
    }
}

