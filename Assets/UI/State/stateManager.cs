
using System;
using System.Collections.Generic;
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

    public NetworkState networkState;


    private StateManager(StateDependencies dependencies) {
        this.dependencies = dependencies;

        this.networkState = new NetworkState(dependencies);
    }
}

