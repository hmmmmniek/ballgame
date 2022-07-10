
using System;
using UnityEngine;

public class UserStateData: StateData {
    public string name;
}

public class UserState: BaseState<UserStateData, UserState> {
    public const string SELECTOR = "Player";

    private StateDependencies dependencies;

    public UserState(StateDependencies dependencies): base() {
        this.dependencies = dependencies;

        state.name = PlayerPrefs.GetString("playerName", string.Empty);
        if(state.name == null || state.name.Length == 0) {
            state.name = getRandomPlayerName();
        }

    }

    private string getRandomPlayerName() {
        System.Random rand = new System.Random();
        int number = rand.Next(0, 999999999);
        string randomString = Convert.ToBase64String(BitConverter.GetBytes(number));
        randomString = randomString.Substring(0, 6);
        return $"Guest-{randomString}";
    }
    
    public static string GetPlayerName(UserStateData state) {
        return state.name;
    }

    public static void SetPlayerName(BaseState<UserStateData, UserState> s, string args, Action c) { (s as UserState).SPN(c, args); }
    private void SPN(Action complete, string args) {
        StateChange((UserStateData state) => {
            state.name = args;
        });
        PlayerPrefs.SetString("playerName", args);
        PlayerPrefs.SetInt("playerNameSet", 1);

    }

}