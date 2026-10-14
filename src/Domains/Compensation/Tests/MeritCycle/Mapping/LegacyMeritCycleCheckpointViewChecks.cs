namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LegacyMeritCycleCheckpointViewChecks
{
    public static void LegacyMeritCycleCheckpointViewShowsTheStatusInCapitals()
    {
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("code-105", "name-501");
        var view = LegacyMeritCycleCheckpointView.ToLegacyMeritCycleCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyMeritCycleCheckpointView.ToLegacyMeritCycleCheckpointViews(new List<LegacyMeritCycleCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyMeritCycleCheckpointViewShowsTheStatusInCapitals();
    }
}
