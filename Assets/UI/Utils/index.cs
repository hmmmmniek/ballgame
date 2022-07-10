using System.Threading.Tasks;
using UnityEngine.UIElements;

public class UIUtils {
    public static void FixListViewScrollingBug(ListView listView) {
        var scroller = listView.Q<Scroller>();
        float scrollVelocity = 0;
        bool scrollerUpdateStarted = false;
        listView.RegisterCallback<WheelEvent>(async @event => {
            scrollVelocity += @event.delta.y * 1000;
            if (!scrollerUpdateStarted) {
                scrollerUpdateStarted = true;
                await ScrollerUpdate(scroller, scrollVelocity);
                scrollerUpdateStarted = false;
            }
            @event.StopPropagation();
        });

    }

    private static async Task ScrollerUpdate(Scroller scroller, float scrollVelocity) {
        await Task.Delay(10);
        scroller.value += scrollVelocity;
        scrollVelocity -= scrollVelocity * 0.1f;

        if (scrollVelocity > 0.01 || scrollVelocity < -0.01) {
            await ScrollerUpdate(scroller, scrollVelocity);
        }
    }
}