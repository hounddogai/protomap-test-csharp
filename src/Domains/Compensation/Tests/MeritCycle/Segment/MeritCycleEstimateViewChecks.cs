namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class MeritCycleEstimateViewChecks
{
    public static void MeritCycleEstimateViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleEstimate.NewMeritCycleEstimate("code-789", "label-453");
        var view = MeritCycleEstimateView.ToMeritCycleEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleEstimateView.ToMeritCycleEstimateViews(new List<MeritCycleEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleEstimateViewShowsTheStatusInCapitals();
    }
}
