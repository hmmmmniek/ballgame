using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseState<T> where T: class, new() {
    protected T state;
    public event EventHandler<T> stateChanged;

    protected BaseState() {
        state = new T();
    }

    protected void StateChange(Action<T> action) {
        action(state);
        stateChanged?.Invoke(this, state);
    }

    protected void StateSelect<R>(Func<T, R> action, Action<R> callback) {
        callback(action(state));
        stateChanged += (object sender, T state) => {
            R result = action(state);
            callback(result);
        };
    }

}