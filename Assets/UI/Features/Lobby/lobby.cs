using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyController : Module {
    public new const string TEMPLATE_SELECTOR = "lobby.uxml";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        BackButtonController.ELEMENT_NAME,
        SessionListItemController.ELEMENT_NAME
    };

    private float scrollVelocity;
    private bool scrollerUpdateStarted;
    private List<SessionInfo> sessions;
    private SessionInfo selectedSession;
    private Button joinButton;
    private VisualElement element;

    public LobbyController(VisualElement element) {
        this.element = element;

        Button regionButton_asia = element.Q<Button>("session-region-selector__asia");
        Button regionButton_jp = element.Q<Button>("session-region-selector__jp");
        Button regionButton_eu = element.Q<Button>("session-region-selector__eu");
        Button regionButton_sa = element.Q<Button>("session-region-selector__sa");
        Button regionButton_us = element.Q<Button>("session-region-selector__us");
        regionButton_asia.clicked += () => { NetworkState.Dispatch(NetworkState.SetRegion, "asia", () => {}); };
        regionButton_jp.clicked += () => { NetworkState.Dispatch(NetworkState.SetRegion, "jp", () => {}); };
        regionButton_eu.clicked += () => { NetworkState.Dispatch(NetworkState.SetRegion, "eu", () => {}); };
        regionButton_sa.clicked += () => { NetworkState.Dispatch(NetworkState.SetRegion, "sa", () => {}); };
        regionButton_us.clicked += () => { NetworkState.Dispatch(NetworkState.SetRegion, "us", () => {}); };

        Watch(NetworkState.Select<string>(NetworkState.GetRegion, (region) => {
            Button unselectedButton = element.Q<VisualElement>(null, "session-region-selector").Q<Button>(null, "selected");
            if(unselectedButton != null) {
                unselectedButton.RemoveFromClassList("selected");
            }
            
            Button selectedButton = element.Q<Button>($"session-region-selector__{region}");
            if(selectedButton != null) {
                selectedButton.AddToClassList("selected");
            }
        }));

        joinButton = element.Q<Button>("JoinButton");
        joinButton.clicked += JoinMatch;

        Button backButton = element.Q<Button>("BackButton");
        backButton.clicked += GoToMainMenu;

        Button createSessionButton = element.Q<Button>("CreateSessionButton");
        createSessionButton.clicked += GoToSessionCreation;

        var listView = element.Q<ListView>();

        SetupSessionList(listView);
        FixListViewScrollingBug(listView);

    }

    private void GoToMainMenu() {
        ViewManager.instance.Open<MainMenuController>();
    }

    private void GoToSessionCreation() {
        ViewManager.instance.Open<CreateSessionController>();
    }

    private void SetupSessionList(ListView listView) {

        Func<VisualElement> makeItem = () => {
            return ViewManager.instance.GetWidgetTemplate<SessionListItemController>().Instantiate();
        };
        Action<VisualElement, int> bindItem = (e, i) => {
            var controller = new SessionListItemController(e);
            controller.Init(sessions[i]);
            e.userData = (controller: controller, type: "widget");
        };

        listView.makeItem = makeItem;
        listView.bindItem = bindItem;
        listView.selectionType = SelectionType.Single;
        listView.onSelectionChange += (e) => {
            foreach (SessionInfo session in e) {
                selectedSession = session;
                joinButton.RemoveFromClassList("hidden");
                return;
            }
        };

        Watch(NetworkState.Select<List<SessionInfo>>(NetworkState.GetSessionList, (sessions) => {
            if (sessions != null) {
                listView.itemsSource = sessions;
                this.sessions = sessions;
                listView.RefreshItems();

                if(selectedSession != null) {
                    var found = false;
                    foreach (SessionInfo session in sessions) {
                        found = found || session.Name == selectedSession.Name;                         
                    }
                    if(!found) {
                        selectedSession = null;
                        joinButton.AddToClassList("hidden");
                    }
                }
            }
        }));
        
    }

    private void JoinMatch() {
        if (selectedSession != null) {
            NetworkState.Dispatch(NetworkState.Join, selectedSession, () => {});
        }
    }

    private void FixListViewScrollingBug(ListView listView) {
        var scroller = listView.Q<Scroller>();

        listView.RegisterCallback<WheelEvent>(async @event => {
            scrollVelocity += @event.delta.y * 1000;
            if (!scrollerUpdateStarted) {
                scrollerUpdateStarted = true;
                await ScrollerUpdate(scroller);
                scrollerUpdateStarted = false;
            }
            @event.StopPropagation();
        });

    }

    private async Task ScrollerUpdate(Scroller scroller) {
        await Task.Delay(10);
        scroller.value += scrollVelocity;
        scrollVelocity -= scrollVelocity * 0.1f;

        if (scrollVelocity > 0.01 || scrollVelocity < -0.01) {
            await ScrollerUpdate(scroller);
        }
    }
}








