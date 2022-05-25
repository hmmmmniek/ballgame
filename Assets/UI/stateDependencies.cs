using Fusion;
using UnityEngine.UIElements;

public class StateDependencies {
    public PlayerSessionManager sessionManager;

    public StateDependencies(
        PlayerSessionManager sessionManager
    ) {
        this.sessionManager = sessionManager;
    }
}
