using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseState<StateDataType, StateType> where StateDataType: StateData, new() {
    public const string TEMPLATE_SELECTOR = "";
    public StateDataType state;
    public event EventHandler<StateDataType> stateChanged;

    protected BaseState() {
        state = new StateDataType();
    }

    public static Action Select<ReturnType>(Func<StateDataType, ReturnType> action, Action<ReturnType> callback) {
        return StateManager.instance.Select<StateType, ReturnType, StateDataType>(action, callback);
    }

    public static ReturnType SelectOnce<ReturnType>(Func<StateDataType, ReturnType> action) {
        return StateManager.instance.SelectOnce<StateType, ReturnType, StateDataType>(action);
    }

    public static void Dispatch<ArgsType>(Action<BaseState<StateDataType, StateType>, ArgsType, Action> action, ArgsType args, Action callback) {
        StateManager.instance.Dispatch<StateType, StateDataType, ArgsType>(action, args, callback);
    }

    protected void StateChange(Action<StateDataType> action) {
        action(state);
        stateChanged?.Invoke(this, state);
    }


}