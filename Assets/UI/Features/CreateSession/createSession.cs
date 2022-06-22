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
    public enum MapSize {
        Small,
        Medium,
        Large
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

    private MapSize _selectedMapSize;
    private MapSize selectedMapSize {
        get {
            return _selectedMapSize;
        }
        set {
            Button unselectedButton = element.Q<VisualElement>(null, "map-size-selector").Q<Button>(null, "selected");
            if(unselectedButton != null) {
                unselectedButton.RemoveFromClassList("selected");
            }
            
            Button selectedButton;
            switch(value) {
                case MapSize.Small: {
                    selectedButton = element.Q<Button>("map-size-selector__small");
                    break;
                }
                case MapSize.Medium: {
                    selectedButton = element.Q<Button>("map-size-selector__medium");
                    break;
                }
                case MapSize.Large: {
                    selectedButton = element.Q<Button>("map-size-selector__large");
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
           
            _selectedMapSize = value;
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

        selectedMapSize = MapSize.Small;

        Button mapSizeButtonSmall = element.Q<Button>("map-size-selector__small");
        Button mapSizeButtonMedium = element.Q<Button>("map-size-selector__medium");
        Button mapSizeButtonLarge = element.Q<Button>("map-size-selector__large");
        mapSizeButtonSmall.clicked += () => { selectedMapSize = MapSize.Small; };
        mapSizeButtonMedium.clicked += () => { selectedMapSize = MapSize.Medium; };
        mapSizeButtonLarge.clicked += () => { selectedMapSize = MapSize.Large; };


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

        NetworkState.Dispatch(NetworkState.Create, (name: sessionName, size: sessionSize, map: selectedMapSize), () => {});
    }
 
    private String getRandomSessionName() {
        System.Random rand = new System.Random();
        int number = rand.Next(0, 999999999);
        string randomString = Convert.ToBase64String(BitConverter.GetBytes(number));
        randomString = randomString.Substring(0, 6);
        return $"Session-{randomString}";
    }
}