using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateSessionController : Module {
    public new const string TEMPLATE_SELECTOR = "createSession.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        BackButtonController.ELEMENT_NAME
    };

    private VisualElement element;
    enum SessionSize {
        _1v1,
        _3v3,
        _5v5
    }
   

    private SessionSize _selectedSessionSize;
    private SessionSize selectedSessionSize {
        get {
            return _selectedSessionSize;
        }
        set {
            Button unselectedButton = element.Q<VisualElement>(null, "session-size-selector").Q<Button>(null, "selected");
            if(unselectedButton != null) {
                unselectedButton.RemoveFromClassList("selected");
            }
            
            Button selectedButton;
            switch(value) {
                case SessionSize._1v1: {
                    selectedButton = element.Q<Button>("session-size-selector__1v1");
                    break;
                }
                case SessionSize._3v3: {
                    selectedButton = element.Q<Button>("session-size-selector__3v3");
                    break;
                }
                case SessionSize._5v5: {
                    selectedButton = element.Q<Button>("session-size-selector__5v5");
                    break;
                }
                default: {
                    selectedButton = null;
                    break;
                }
            }
            if(selectedButton != null) {
                selectedButton.AddToClassList("selected");
            }
           
            _selectedSessionSize = value;
        }
    }

    private string sessionName;

    public CreateSessionController(VisualElement element) {
        this.element = element;

        Button backButton = element.Q<Button>("BackButton");
        backButton.clicked += GoToLobby;

        selectedSessionSize = SessionSize._3v3;

        Button sessionSizeButton1v1 = element.Q<Button>("session-size-selector__1v1");
        Button sessionSizeButton3v3 = element.Q<Button>("session-size-selector__3v3");
        Button sessionSizeButton5v5 = element.Q<Button>("session-size-selector__5v5");
        sessionSizeButton1v1.clicked += () => { selectedSessionSize = SessionSize._1v1; };
        sessionSizeButton3v3.clicked += () => { selectedSessionSize = SessionSize._3v3; };
        sessionSizeButton5v5.clicked += () => { selectedSessionSize = SessionSize._5v5; };

        TextField sessionNameInput = element.Q<TextField>("session-name-input__textfield");
        sessionNameInput.RegisterValueChangedCallback((change) => {
            sessionName = change.newValue;
        });
        sessionNameInput.value = getRandomSessionName();
        sessionName = sessionNameInput.value;


        Button sessionNameClearButton = element.Q<Button>("session-name-input__clear");
        sessionNameClearButton.clicked += () => { sessionNameInput.value = ""; };

        Button sessionCreateButton = element.Q<Button>("session-create__button");
        sessionCreateButton.clicked += StartSession;

        


    }
    private void GoToLobby() {
        ViewManager.instance.Open<LobbyController>();
    }

    private void StartSession() {
        int sessionSize = 0;
        switch(selectedSessionSize) {
            case SessionSize._1v1: {
                sessionSize = 2;
                break;
            }
            case SessionSize._3v3: {
                sessionSize = 6;
                break;
            }
            case SessionSize._5v5: {
                sessionSize = 10;
                break;
            }
        }
        NetworkState.Dispatch(NetworkState.Create, (name: sessionName, size: sessionSize), () => {});
    }
 
    private String getRandomSessionName() {
        System.Random rand = new System.Random();
        int number = rand.Next(0, 999999999);
        string randomString = Convert.ToBase64String(BitConverter.GetBytes(number));
        randomString = randomString.Substring(0, 6);
        return $"Session-{randomString}";
    }
}