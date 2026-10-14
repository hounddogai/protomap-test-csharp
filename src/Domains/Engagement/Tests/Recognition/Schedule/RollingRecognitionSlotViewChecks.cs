namespace Acme.Workforce.Engagement.Tests.Recognition.Schedule;

using Acme.Workforce.Engagement.Recognition.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RollingRecognitionSlotViewChecks
{
    public static void RollingRecognitionSlotViewShowsTheStatusInCapitals()
    {
        var item = RollingRecognitionSlot.NewRollingRecognitionSlot("code-886", "label-843");
        var view = RollingRecognitionSlotView.ToRollingRecognitionSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingRecognitionSlotView.ToRollingRecognitionSlotViews(new List<RollingRecognitionSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingRecognitionSlotViewShowsTheStatusInCapitals();
    }
}
