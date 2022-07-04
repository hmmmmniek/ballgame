using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class TeamSelectController : Module {
    public new const string TEMPLATE_SELECTOR = "teamSelect.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
    };

    private VisualElement element;
   
    private Player localPlayer;

    private Team _selectedTeam;
    private Team selectedTeam {
        get {
            return _selectedTeam;
        }
        set {
            Button unselectedButton = element.Q<VisualElement>(null, "team-selector").Q<Button>(null, "selected");
            if(unselectedButton != null) {
                unselectedButton.RemoveFromClassList("selected");
            }
            
            Button selectedButton;
            switch(value) {
                case Team.Blue: {
                    selectedButton = element.Q<Button>("team-selector__blue");
                    break;
                }
                case Team.Red: {
                    selectedButton = element.Q<Button>("team-selector__red");
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
           
            _selectedTeam = value;
        }
    }



    public TeamSelectController(VisualElement element) {
        this.element = element;

        selectedTeam = Team.Blue;

        Button teamButtonBlue = element.Q<Button>("team-selector__blue");
        Button teamButtonRed = element.Q<Button>("team-selector__red");
        teamButtonBlue.clicked += () => { selectedTeam = Team.Blue; };
        teamButtonRed.clicked += () => { selectedTeam = Team.Red; };

        Button joinButton = element.Q<Button>("join__button");
        joinButton.clicked += JoinSession;

        Button leaveButton = element.Q<Button>("leave__button");
        leaveButton.clicked += LeaveSession;

        Button spectateButton = element.Q<Button>("spectate__button");
        spectateButton.clicked += StartSpectate;
        
        Button settingsButton = element.Q<Button>("settings__button");
        settingsButton.clicked += GoToSettings;
        


        Watch(GameState.Select<Player[]>(GameState.GetPlayers, (players) => {
            foreach (var player in players) {
                if(player.isLocal) {
                    localPlayer = player;
                }
            }
        }));
    }


    private void JoinSession() {
        MatchController.instance.ChangeTeam(localPlayer.playerRef.Value, selectedTeam);
        ViewManager.instance.Open<GameController>();

    }
 
    private void GoToSettings() {
        SettingsController controller = ViewManager.instance.Open<SettingsController>();
        controller.backAction = () => {
            ViewManager.instance.Open<TeamSelectController>();
        };
    }

    private void LeaveSession() {
        NetworkState.Dispatch<object>(NetworkState.Leave, null, () => {});
        ViewManager.instance.Open<MainMenuController>();

    }
    private void StartSpectate() {
        ViewManager.instance.Open<SpectateController>();

    }
}
