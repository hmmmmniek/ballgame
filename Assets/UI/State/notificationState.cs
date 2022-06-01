using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public enum NotificationUrgency {
    Error,
    Warning,
    Info
}
public class NotificationStateData: StateData {
    public (NotificationUrgency urgency, string text)? currentNotification;
}

public class NotificationState: BaseState<NotificationStateData, NotificationState> {
    public const string SELECTOR = "Notification";

    private StateDependencies dependencies;

    public NotificationState(StateDependencies dependencies): base() {
        this.dependencies = dependencies;
        state.currentNotification = null;
    }
    
    public static (NotificationUrgency urgency, string text)? GetCurrentNotification(NotificationStateData state) {
        return state.currentNotification;
    }



    public static void Notify(BaseState<NotificationStateData, NotificationState> s, (NotificationUrgency urgency, string text) args, Action c) { (s as NotificationState).N(c, args); }
    private void N(Action complete, (NotificationUrgency urgency, string text) args) {
        StateChange((NotificationStateData state) => {
            state.currentNotification = args;
        });
    }


}