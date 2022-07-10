using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class ScoreboardController : Widget {
    public new const string ELEMENT_NAME = "Scoreboard";
    public new const string TEMPLATE_SELECTOR = "scoreboard.uxml";
    private VisualElement element;
    private VisualElement scoreboardContainer;
     private Label blueScoreLabel;
    private Label redScoreLabel;
    private Label minutesLabel;
    private Label secondsLabel;
    private float matchEnd = Time.time;
    private List<Player> bluePlayers = new List<Player>();
    private List<Player> redPlayers = new List<Player>();
    private List<Player> spectatingPlayers = new List<Player>();

    public ScoreboardController(VisualElement element) {
        this.element = element;
        scoreboardContainer = element.Q<VisualElement>("scoreboard__container");
        Watch(InputState.Select(InputState.GetShowScoreboard, (showScoreboard) => {
            if(showScoreboard) {
                scoreboardContainer.RemoveFromClassList("hidden");
            } else if(!scoreboardContainer.ClassListContains("hidden")) {
                scoreboardContainer.AddToClassList("hidden");
            }
        }));

        blueScoreLabel = element.Q<Label>("scoreboard__teams__blue__score__score-label");
        redScoreLabel = element.Q<Label>("scoreboard__teams__red__score__score-label");
        minutesLabel = element.Q<Label>("scoreboard__time__minutes-label");
        secondsLabel = element.Q<Label>("scoreboard__time__seconds-label");
        Watch(GameState.Select(GameState.GetBlueTeamScore, (score) => {
            blueScoreLabel.text = $"{score}";
        }));
        Watch(GameState.Select(GameState.GetRedTeamScore, (score) => {
            redScoreLabel.text = $"{score}";
        }));
        Watch(GameState.Select(GameState.GetMatchEnd, (matchEndTime) => {
            matchEnd = matchEndTime;
        }));

        SetupPlayersList(Team.Blue, element.Q<ListView>("scoreboard__teams__blue__players"), bluePlayers);
        SetupPlayersList(Team.Red, element.Q<ListView>("scoreboard__teams__red__players"), redPlayers);
        SetupPlayersList(Team._, element.Q<ListView>("scoreboard__spectators__list"), spectatingPlayers);

        UpdateTime();
    }

    private async void UpdateTime() {
        await Task.Delay(100);

        float remainingSeconds = matchEnd - Time.time;
        int seconds = (int)Math.Floor(remainingSeconds % 60f);
        int minutes = (int)((remainingSeconds - seconds) / 60f);
        minutesLabel.text = $"{(minutes < 10 ? "0" : "")}{minutes}";
        secondsLabel.text = $"{(seconds < 10 ? "0" : "")}{seconds}";

        UpdateTime();
    }


    private void SetupPlayersList(Team team, ListView listView, List<Player> players) {

        Func<VisualElement> makeItem = () => {
            return ViewManager.instance.GetWidgetTemplate<PlayerListItemController>().Instantiate();
        };
        Action<VisualElement, int> bindItem = (e, i) => {
            var controller = new PlayerListItemController(e);
            controller.Init(players[i]);
            e.userData = (controller: controller, type: "widget");
        };


        listView.makeItem = makeItem;
        listView.bindItem = bindItem;
        listView.selectionType = SelectionType.None;

        Watch(GameState.Select<Player[]>(GameState.GetPlayers, (p) => {
            if (p != null && p.Length > 0) {
                IEnumerable<Player> teamPlayers = p.Where(p => p.team == team);
                listView.itemsSource = teamPlayers.ToArray();
                players.Clear();
                foreach (var player in teamPlayers) {
                    players.Add(player);
                }
                listView.RefreshItems();
            }
        }));

        UIUtils.FixListViewScrollingBug(listView);
    }
}