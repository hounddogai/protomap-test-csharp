namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class MeritCycleBaselineViewChecks
{
    public static void MeritCycleBaselineViewShowsTheStatusInCapitals()
    {
        var item = MeritCycleBaseline.NewMeritCycleBaseline("code-972", "name-320");
        var view = MeritCycleBaselineView.ToMeritCycleBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MeritCycleBaselineView.ToMeritCycleBaselineViews(new List<MeritCycleBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MeritCycleBaselineViewShowsTheStatusInCapitals();
    }
}
