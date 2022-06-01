
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StateManager {
    private static StateManager _instance = null;
    public static StateManager instance {
        get {
            return _instance;
        }
    }
    public static void Init(StateDependencies dependencies) {
        _instance = new StateManager(dependencies);
    }
    private StateDependencies dependencies;

    Dictionary<string, object> states = new Dictionary<string, object>();


    private StateManager(StateDependencies dependencies) {
        this.dependencies = dependencies;

        states.Add(NetworkState.SELECTOR, new NetworkState(dependencies));
        states.Add(InputState.SELECTOR, new InputState(dependencies));
    }

    public Action Select<StateType, ReturnType, StateDataType>(Func<StateDataType, ReturnType> action, Action<ReturnType> callback) where StateDataType: StateData, new() {
        BaseState<StateDataType, StateType> state = GetState<StateType, StateDataType>();
        callback(action(state.state));
        EventHandler<StateDataType> handler = (object sender, StateDataType state) => {
            ReturnType result = action(state);
            callback(result);
        };
        state.stateChanged += handler;
        return () => {
            state.stateChanged -= handler;
        };
    }

    public ReturnType SelectOnce<StateType, ReturnType, StateDataType>(Func<StateDataType, ReturnType> action) where StateDataType: StateData, new() {
        BaseState<StateDataType, StateType> state = GetState<StateType, StateDataType>();
        ReturnType result = action(state.state);
        return result;
    }

    public void Dispatch<StateType, StateDataType, ArgsType>(Action<BaseState<StateDataType, StateType>, ArgsType, Action> action, ArgsType args, Action callback) where StateDataType: StateData, new()  {
        BaseState<StateDataType, StateType> state = GetState<StateType, StateDataType>();
        action(state, args, callback);
    }

    public BaseState<StateDataType, StateType> GetState<StateType, StateDataType>() where StateDataType: StateData, new() {
        return states[typeof(StateType).GetField("SELECTOR").GetValue(null) as string] as BaseState<StateDataType, StateType>;
    }
}

