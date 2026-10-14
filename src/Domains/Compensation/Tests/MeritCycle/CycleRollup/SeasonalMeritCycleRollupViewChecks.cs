namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class SeasonalMeritCycleRollupViewChecks
{
    public static void SeasonalMeritCycleRollupViewShowsTheStatusInCapitals()
    {
        var item = SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup("code-324", "title-818");
        var view = SeasonalMeritCycleRollupView.ToSeasonalMeritCycleRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalMeritCycleRollupView.ToSeasonalMeritCycleRollupViews(new List<SeasonalMeritCycleRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalMeritCycleRollupViewShowsTheStatusInCapitals();
    }
}
