namespace Acme.Workforce.Orgdesign.Tests.Scenario.Tier;

using Acme.Workforce.Orgdesign.Scenario.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ScenarioBaselineChecks
{
    public static void ScenarioBaselineStartsInCreated()
    {
        var item = ScenarioBaseline.NewScenarioBaseline("code-318", "label-136");
        Check.Equal(item.Status, ScenarioBaselineStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioBaselineMovesToValidated()
    {
        var item = ScenarioBaseline.NewScenarioBaseline("code-318", "label-136");
        try
        {
            item.MoveTo(ScenarioBaselineStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioBaselineStatus.Validated, "status");
    }

    public static void ScenarioBaselineRejectsMovesFromRetired()
    {
        var item = ScenarioBaseline.NewScenarioBaseline("code-318", "label-136");
        item.Status = ScenarioBaselineStatus.Retired;
        Check.Fails(() => item.MoveTo(ScenarioBaselineStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScenarioBaselineStartsInCreated();
        ScenarioBaselineMovesToValidated();
        ScenarioBaselineRejectsMovesFromRetired();
    }
}
