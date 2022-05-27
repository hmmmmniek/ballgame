using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.UIElements;

public class LobbyController : Module {
    public new readonly static string TEMPLATE_SELECTOR = "lobbyTemplate";
    public new readonly static IEnumerable<string> WIDGET_ELEMENT_NAMES = new string[] {
        BackToMainController.ELEMENT_NAME,
        SessionListItemController.ELEMENT_NAME
    };

    private float scrollVelocity;
    private bool scrollerUpdateStarted;
    private List<SessionInfo> sessions;
    private SessionInfo selectedSession;
    private Button joinButton;
    public LobbyController(VisualElement element) {

        joinButton = element.Q<Button>("JoinButton");
        joinButton.clicked += JoinMatch;

        var listView = element.Q<ListView>();

        SetupSessionList(listView);
        FixListViewScrollingBug(listView);


    }

    private void SetupSessionList(ListView listView) {

        Func<VisualElement> makeItem = () => {
            return ViewManager.instance.GetWidgetTemplate<SessionListItemController>().Instantiate();
        };
        Action<VisualElement, int> bindItem = (e, i) => {
            var controller = new SessionListItemController(e);
            controller.Init(sessions[i]);
            e.userData = controller;
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

        StateManager.instance.networkState.E_GetSessionList((sessions => {
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

    private async void JoinMatch() {
        if (selectedSession != null) {
            await StateManager.instance.networkState.Join(selectedSession);
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
