using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InputStateData: StateData {
    public string bindingsJson;
    public InputActionAsset actions;
}

public class InputState: BaseState<InputStateData, InputState> {
    public const string SELECTOR = "Input";

    private StateDependencies dependencies;


    private InputAction jumpAction;

    public InputState(StateDependencies dependencies): base() {
        this.dependencies = dependencies;

        jumpAction = dependencies.playerInput.actions.FindAction("Jump");

        state.bindingsJson = PlayerPrefs.GetString("bindings", string.Empty);
        state.actions = dependencies.playerInput.actions;
        if(!string.IsNullOrEmpty(state.bindingsJson)) {
            dependencies.playerInput.actions.LoadBindingOverridesFromJson(state.bindingsJson);
        }

    }

    public static Func<string, string> GetBindingLabelFn(InputStateData state) {
        return (string actionName) => {
            InputAction action = state.actions.FindAction(actionName);
            return InputState.GetInputActionLabel(action);
        };
    }

    public static string GetBindingsJson(InputStateData state) {
        return state.bindingsJson;
    }

    public static void StartRebind(BaseState<InputStateData, InputState> s, string args, Action c) { (s as InputState).SRJ(c, args); }
    private void SRJ(Action complete, string bindingName) {
        InputAction action = state.actions.FindAction(bindingName);

        dependencies.playerInput.SwitchCurrentActionMap("Rebinding");
        action
            .PerformInteractiveRebinding()
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f)
            .OnCancel(operation => {
                operation.Dispose();
                dependencies.playerInput.SwitchCurrentActionMap("Player");
                complete();
            })
            .OnComplete((operation) => {
                operation.Dispose();
                dependencies.playerInput.SwitchCurrentActionMap("Player");
                ApplyBindings();
                complete();
            })
            .Start();
    }

    public static void ResetRebind(BaseState<InputStateData, InputState> s, string args, Action c) { (s as InputState).RRJ(c, args); }
    private void RRJ(Action complete, string bindingName) {
        InputAction action = state.actions.FindAction(bindingName);
        action.RemoveAllBindingOverrides();
        ApplyBindings();
    }

    private void ApplyBindings() {
        string bindings = dependencies.playerInput.actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("bindings", bindings);

        StateChange((InputStateData state) => {
            state.bindingsJson = bindings;
        });
    }
    

    private static string GetInputActionLabel(InputAction action) {
        int bindingIndex = action.GetBindingIndexForControl(action.controls[0]);
        return InputControlPath.ToHumanReadableString(action.bindings[bindingIndex].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
    }




}