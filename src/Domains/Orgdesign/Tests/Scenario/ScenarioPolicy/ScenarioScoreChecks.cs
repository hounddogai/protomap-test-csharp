namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioScoreChecks
{
    public static void ScenarioScoreStartsInProposed()
    {
        var item = ScenarioScore.NewScenarioScore("code-437", "name-680");
        Check.Equal(item.Status, ScenarioScoreStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioScoreMovesToApproved()
    {
        var item = ScenarioScore.NewScenarioScore("code-437", "name-680");
        try
        {
            item.MoveTo(ScenarioScoreStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioScoreStatus.Approved, "status");
    }

    public static void ScenarioScoreRejectsMovesFromWithdrawn()
    {
        var item = ScenarioScore.NewScenarioScore("code-437", "name-680");
        item.Status = ScenarioScoreStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScenarioScoreStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScenarioScoreStartsInProposed();
        ScenarioScoreMovesToApproved();
        ScenarioScoreRejectsMovesFromWithdrawn();
    }
}
