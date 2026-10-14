namespace Acme.Workforce.Compensation.Tests.MeritCycle.Metric;

using Acme.Workforce.Compensation.MeritCycle.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredMeritCycleCheckpointViewChecks
{
    public static void TieredMeritCycleCheckpointViewShowsTheStatusInCapitals()
    {
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("code-661", "title-457");
        var view = TieredMeritCycleCheckpointView.ToTieredMeritCycleCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredMeritCycleCheckpointView.ToTieredMeritCycleCheckpointViews(new List<TieredMeritCycleCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredMeritCycleCheckpointViewShowsTheStatusInCapitals();
    }
}
