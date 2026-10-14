namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class ScenarioTargetChecks
{
    public static void ScenarioTargetStartsInProposed()
    {
        var item = ScenarioTarget.NewScenarioTarget("code-924", "name-949");
        Check.Equal(item.Status, ScenarioTargetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioTargetMovesToApproved()
    {
        var item = ScenarioTarget.NewScenarioTarget("code-924", "name-949");
        try
        {
            item.MoveTo(ScenarioTargetStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioTargetStatus.Approved, "status");
    }

    public static void ScenarioTargetRejectsMovesFromWithdrawn()
    {
        var item = ScenarioTarget.NewScenarioTarget("code-924", "name-949");
        item.Status = ScenarioTargetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScenarioTargetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScenarioTargetStartsInProposed();
        ScenarioTargetMovesToApproved();
        ScenarioTargetRejectsMovesFromWithdrawn();
    }
}
