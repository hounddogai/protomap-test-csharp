namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioMappingChecks
{
    public static void ScenarioMappingStartsInCreated()
    {
        var item = ScenarioMapping.NewScenarioMapping("code-382", "name-157");
        Check.Equal(item.Status, ScenarioMappingStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioMappingMovesToPublished()
    {
        var item = ScenarioMapping.NewScenarioMapping("code-382", "name-157");
        try
        {
            item.MoveTo(ScenarioMappingStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioMappingStatus.Published, "status");
    }

    public static void ScenarioMappingRejectsMovesFromRetired()
    {
        var item = ScenarioMapping.NewScenarioMapping("code-382", "name-157");
        item.Status = ScenarioMappingStatus.Retired;
        Check.Fails(() => item.MoveTo(ScenarioMappingStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScenarioMappingStartsInCreated();
        ScenarioMappingMovesToPublished();
        ScenarioMappingRejectsMovesFromRetired();
    }
}
