namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class ScheduledReorgThresholdViewChecks
{
    public static void ScheduledReorgThresholdViewShowsTheStatusInCapitals()
    {
        var item = ScheduledReorgThreshold.NewScheduledReorgThreshold("code-175", "label-482");
        var view = ScheduledReorgThresholdView.ToScheduledReorgThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledReorgThresholdView.ToScheduledReorgThresholdViews(new List<ScheduledReorgThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledReorgThresholdViewShowsTheStatusInCapitals();
    }
}
