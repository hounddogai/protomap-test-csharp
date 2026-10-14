namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class WeightedMeritCycleSegmentViewChecks
{
    public static void WeightedMeritCycleSegmentViewShowsTheStatusInCapitals()
    {
        var item = WeightedMeritCycleSegment.NewWeightedMeritCycleSegment("code-441", "title-829");
        var view = WeightedMeritCycleSegmentView.ToWeightedMeritCycleSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedMeritCycleSegmentView.ToWeightedMeritCycleSegmentViews(new List<WeightedMeritCycleSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedMeritCycleSegmentViewShowsTheStatusInCapitals();
    }
}
