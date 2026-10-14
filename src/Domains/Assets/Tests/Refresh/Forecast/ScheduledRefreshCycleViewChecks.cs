namespace Acme.Workforce.Assets.Tests.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledRefreshCycleViewChecks
{
    public static void ScheduledRefreshCycleViewShowsTheStatusInCapitals()
    {
        var item = ScheduledRefreshCycle.NewScheduledRefreshCycle("code-306", "label-989");
        var view = ScheduledRefreshCycleView.ToScheduledRefreshCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledRefreshCycleView.ToScheduledRefreshCycleViews(new List<ScheduledRefreshCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledRefreshCycleViewShowsTheStatusInCapitals();
    }
}
