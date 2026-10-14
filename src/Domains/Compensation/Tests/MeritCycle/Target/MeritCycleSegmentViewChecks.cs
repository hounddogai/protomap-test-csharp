namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleSegmentViewChecks
{
    public static void MeritCycleSegmentViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleSegment.NewMeritCycleSegment("code-129", "title-371");
        var view = MeritCycleSegmentView.ToMeritCycleSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleSegmentView.ToMeritCycleSegmentViews(new List<MeritCycleSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleSegmentViewShowsTheStatusInCapitals();
    }
}
