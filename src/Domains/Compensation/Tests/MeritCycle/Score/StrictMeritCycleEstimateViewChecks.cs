namespace Acme.Workforce.Compensation.Tests.MeritCycle.Score;

using Acme.Workforce.Compensation.MeritCycle.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictMeritCycleEstimateViewChecks
{
    public static void StrictMeritCycleEstimateViewShowsTheStatusInCapitals()
    {
        var item = StrictMeritCycleEstimate.NewStrictMeritCycleEstimate("code-577", "label-745");
        var view = StrictMeritCycleEstimateView.ToStrictMeritCycleEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictMeritCycleEstimateView.ToStrictMeritCycleEstimateViews(new List<StrictMeritCycleEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictMeritCycleEstimateViewShowsTheStatusInCapitals();
    }
}
