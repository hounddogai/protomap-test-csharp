namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioTemplate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

/// <summary>Checks of scenario template code, run by the project's runner.</summary>
public static class PendingScenarioTemplateChecks
{
    public static void PendingScenarioTemplateStartsInProposed()
    {
        var item = PendingScenarioTemplate.NewPendingScenarioTemplate("code-847", "name-335");
        Check.Equal(item.Status, PendingScenarioTemplateStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingScenarioTemplateMovesToRejected()
    {
        var item = PendingScenarioTemplate.NewPendingScenarioTemplate("code-847", "name-335");
        try
        {
            item.MoveTo(PendingScenarioTemplateStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingScenarioTemplateStatus.Rejected, "status");
    }

    public static void PendingScenarioTemplateRejectsMovesFromWithdrawn()
    {
        var item = PendingScenarioTemplate.NewPendingScenarioTemplate("code-847", "name-335");
        item.Status = PendingScenarioTemplateStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingScenarioTemplateStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingScenarioTemplateStartsInProposed();
        PendingScenarioTemplateMovesToRejected();
        PendingScenarioTemplateRejectsMovesFromWithdrawn();
    }
}
