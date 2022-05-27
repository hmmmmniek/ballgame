using Fusion;
using UnityEngine.UIElements;

public class StateDependencies {
    public NetworkManager networkManager;

    public StateDependencies(
        NetworkManager networkManager
    ) {
        this.networkManager = networkManager;
    }
}
