namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleReview;

using Acme.Workforce.Compensation.MeritCycle.CycleReview;

/// <summary>Checks of cycle review code, run by the project's runner.</summary>
public static class WeightedMeritCycleTierViewChecks
{
    public static void WeightedMeritCycleTierViewShowsTheStatusInCapitals()
    {
        var item = WeightedMeritCycleTier.NewWeightedMeritCycleTier("code-176", "label-675");
        var view = WeightedMeritCycleTierView.ToWeightedMeritCycleTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedMeritCycleTierView.ToWeightedMeritCycleTierViews(new List<WeightedMeritCycleTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedMeritCycleTierViewShowsTheStatusInCapitals();
    }
}
