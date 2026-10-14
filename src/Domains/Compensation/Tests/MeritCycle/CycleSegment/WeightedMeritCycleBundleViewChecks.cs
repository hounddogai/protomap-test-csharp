namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class WeightedMeritCycleBundleViewChecks
{
    public static void WeightedMeritCycleBundleViewShowsTheStatusInCapitals()
    {
        var item = WeightedMeritCycleBundle.NewWeightedMeritCycleBundle("code-408", "label-474");
        var view = WeightedMeritCycleBundleView.ToWeightedMeritCycleBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedMeritCycleBundleView.ToWeightedMeritCycleBundleViews(new List<WeightedMeritCycleBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedMeritCycleBundleViewShowsTheStatusInCapitals();
    }
}
