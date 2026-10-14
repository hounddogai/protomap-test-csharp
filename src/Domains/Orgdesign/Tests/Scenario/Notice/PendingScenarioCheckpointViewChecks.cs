namespace Acme.Workforce.Orgdesign.Tests.Scenario.Notice;

using Acme.Workforce.Orgdesign.Scenario.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class PendingScenarioCheckpointViewChecks
{
    public static void PendingScenarioCheckpointViewShowsTheStatusInCapitals()
    {
        var item = PendingScenarioCheckpoint.NewPendingScenarioCheckpoint("code-286", "label-752");
        var view = PendingScenarioCheckpointView.ToPendingScenarioCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingScenarioCheckpointView.ToPendingScenarioCheckpointViews(new List<PendingScenarioCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingScenarioCheckpointViewShowsTheStatusInCapitals();
    }
}
