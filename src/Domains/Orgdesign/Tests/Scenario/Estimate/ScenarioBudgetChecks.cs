namespace Acme.Workforce.Orgdesign.Tests.Scenario.Estimate;

using Acme.Workforce.Orgdesign.Scenario.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class ScenarioBudgetChecks
{
    public static void ScenarioBudgetStartsInProposed()
    {
        var item = ScenarioBudget.NewScenarioBudget("code-279", "label-139");
        Check.Equal(item.Status, ScenarioBudgetStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioBudgetMovesToRejected()
    {
        var item = ScenarioBudget.NewScenarioBudget("code-279", "label-139");
        try
        {
            item.MoveTo(ScenarioBudgetStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioBudgetStatus.Rejected, "status");
    }

    public static void ScenarioBudgetRejectsMovesFromWithdrawn()
    {
        var item = ScenarioBudget.NewScenarioBudget("code-279", "label-139");
        item.Status = ScenarioBudgetStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScenarioBudgetStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScenarioBudgetStartsInProposed();
        ScenarioBudgetMovesToRejected();
        ScenarioBudgetRejectsMovesFromWithdrawn();
    }
}
