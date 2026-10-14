namespace Acme.Workforce.Compensation.Tests.MeritCycle.Rollup;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryMeritCycleRollupViewChecks
{
    public static void PrimaryMeritCycleRollupViewShowsTheStatusInCapitals()
    {
        var item = PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup("code-787", "title-879");
        var view = PrimaryMeritCycleRollupView.ToPrimaryMeritCycleRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryMeritCycleRollupView.ToPrimaryMeritCycleRollupViews(new List<PrimaryMeritCycleRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryMeritCycleRollupViewShowsTheStatusInCapitals();
    }
}
