using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsController : Module {
    public new const string TEMPLATE_SELECTOR = "settings.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        BackButtonController.ELEMENT_NAME
    };
    private VisualElement element;
    

    public SettingsController(VisualElement element) {

        this.element = element;

        Button backButton = element.Q<Button>("BackButton");
        backButton.clicked += GoToMainMenu;

        Button jumpButton = element.Q<Button>("input-rebinding_jump-button");
        jumpButton.clicked += StartRebindJump;

        Button jumpButtonReset = element.Q<Button>("input-rebinding_jump-button-reset");
        jumpButtonReset.clicked += ResetJumpButton;

        Watch(InputState.Select<Func<string, string>>(InputState.GetBindingLabelFn, (GetBindingLabel) => {
            jumpButton.text = GetBindingLabel("Jump");
        }));
        
    }


    private void GoToMainMenu() {
        ViewManager.instance.Open<MainMenuController>();
    }

    private void StartRebindJump() {
        Button jumpButton = element.Q<Button>("input-rebinding_jump-button");
        jumpButton.AddToClassList("hidden");

        Label jumpButtonWaitingLabel = element.Q<Label>("input-rebinding_jump-button-waiting-label");
        jumpButtonWaitingLabel.RemoveFromClassList("hidden");
        InputState.Dispatch(InputState.StartRebind, "Jump", () => {
            jumpButtonWaitingLabel.AddToClassList("hidden");
            jumpButton.RemoveFromClassList("hidden");
        });

    }

    private void ResetJumpButton() {
        InputState.Dispatch(InputState.ResetRebind, "Jump", () => { });
    }

}
