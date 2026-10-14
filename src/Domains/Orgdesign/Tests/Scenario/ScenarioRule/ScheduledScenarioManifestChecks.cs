namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

/// <summary>Checks of scenario rule code, run by the project's runner.</summary>
public static class ScheduledScenarioManifestChecks
{
    public static void ScheduledScenarioManifestStartsInDraft()
    {
        var item = ScheduledScenarioManifest.NewScheduledScenarioManifest("code-75", "name-699");
        Check.Equal(item.Status, ScheduledScenarioManifestStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledScenarioManifestMovesToActive()
    {
        var item = ScheduledScenarioManifest.NewScheduledScenarioManifest("code-75", "name-699");
        try
        {
            item.MoveTo(ScheduledScenarioManifestStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledScenarioManifestStatus.Active, "status");
    }

    public static void ScheduledScenarioManifestRejectsMovesFromClosed()
    {
        var item = ScheduledScenarioManifest.NewScheduledScenarioManifest("code-75", "name-699");
        item.Status = ScheduledScenarioManifestStatus.Closed;
        Check.Fails(() => item.MoveTo(ScheduledScenarioManifestStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ScheduledScenarioManifestStartsInDraft();
        ScheduledScenarioManifestMovesToActive();
        ScheduledScenarioManifestRejectsMovesFromClosed();
    }
}
