using System;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{

    public NetworkManager networkManager;
    public PlayerInput playerInput;
    private Player? localPlayer;

    private void OnEnable() {
        var stateDependencies = new StateDependencies(
            networkManager,
            playerInput
        );
        StateManager.Init(stateDependencies);

        var uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;

        ViewManager.Init(root.Q<VisualElement>("Root"));
       

        GameState.Select<Player[]>(GameState.GetPlayers, (players) => {
            localPlayer = null;
            foreach (var player in players) {
                if(player.isLocal) {
                    localPlayer = player;
                }
            }
        });


        ShowMenu();

    }

    public void ShowMenu() {
        if(!localPlayer.HasValue) {
            ViewManager.instance.Open<MainMenuController>();
        } else {
            ViewManager.instance.Open<InGameMenuController>();
        }
    }

}  


