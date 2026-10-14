namespace Acme.Workforce.Talent.Tests.Pool.Exception;

using Acme.Workforce.Talent.Pool.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ScheduledPoolThresholdViewChecks
{
    public static void ScheduledPoolThresholdViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPoolThreshold.NewScheduledPoolThreshold("code-749", "title-268");
        var view = ScheduledPoolThresholdView.ToScheduledPoolThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPoolThresholdView.ToScheduledPoolThresholdViews(new List<ScheduledPoolThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPoolThresholdViewShowsTheStatusInCapitals();
    }
}
