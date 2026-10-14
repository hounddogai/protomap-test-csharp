namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioPolicy;

using Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

/// <summary>Checks of scenario policy code, run by the project's runner.</summary>
public static class ScenarioEstimateChecks
{
    public static void ScenarioEstimateStartsInCreated()
    {
        var item = ScenarioEstimate.NewScenarioEstimate("code-286", "name-67");
        Check.Equal(item.Status, ScenarioEstimateStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioEstimateMovesToPublished()
    {
        var item = ScenarioEstimate.NewScenarioEstimate("code-286", "name-67");
        try
        {
            item.MoveTo(ScenarioEstimateStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioEstimateStatus.Published, "status");
    }

    public static void ScenarioEstimateRejectsMovesFromRetired()
    {
        var item = ScenarioEstimate.NewScenarioEstimate("code-286", "name-67");
        item.Status = ScenarioEstimateStatus.Retired;
        Check.Fails(() => item.MoveTo(ScenarioEstimateStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScenarioEstimateStartsInCreated();
        ScenarioEstimateMovesToPublished();
        ScenarioEstimateRejectsMovesFromRetired();
    }
}
