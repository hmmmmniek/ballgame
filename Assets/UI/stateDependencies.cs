using Fusion;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class StateDependencies {
    public NetworkManager networkManager;
    public PlayerInput playerInput;

    public StateDependencies(
        NetworkManager networkManager,
        PlayerInput playerInput
    ) {
        this.networkManager = networkManager;
        this.playerInput = playerInput;
    }
}
