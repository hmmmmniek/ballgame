
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

        AddState<NetworkState>(dependencies);
        AddState<InputState>(dependencies);
        AddState<NotificationState>(dependencies);
        AddState<GameState>(dependencies);
        AddState<NetworkStatsState>(dependencies);
        AddState<UserState>(dependencies);
    }

    private void AddState<StateType>(StateDependencies dependencies) {
        string key = typeof(StateType).GetField("SELECTOR").GetValue(null) as string;
        object state = Activator.CreateInstance(typeof(StateType), new object[] { dependencies });
        states.Add(key, state);
    }

    public Action Select<StateType, ReturnType, StateDataType>(Func<StateDataType, ReturnType> action, Action<ReturnType> callback) where StateDataType: StateData, new() {
        BaseState<StateDataType, StateType> state = GetState<StateType, StateDataType>();
        ReturnType value = action(state.state);
        callback(value);

        EventHandler<StateDataType> handler = (object sender, StateDataType state) => {
            ReturnType newValue = action(state);

            if(!((value == null && newValue == null) || (value != null && value.Equals(newValue)))) {
                value = newValue;
                callback(value);
            }
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

