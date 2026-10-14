namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioCycleChecks
{
    public static void ScenarioCycleStartsInCreated()
    {
        var item = ScenarioCycle.NewScenarioCycle("code-763", "label-671");
        Check.Equal(item.Status, ScenarioCycleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioCycleMovesToPublished()
    {
        var item = ScenarioCycle.NewScenarioCycle("code-763", "label-671");
        try
        {
            item.MoveTo(ScenarioCycleStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioCycleStatus.Published, "status");
    }

    public static void ScenarioCycleRejectsMovesFromRetired()
    {
        var item = ScenarioCycle.NewScenarioCycle("code-763", "label-671");
        item.Status = ScenarioCycleStatus.Retired;
        Check.Fails(() => item.MoveTo(ScenarioCycleStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScenarioCycleStartsInCreated();
        ScenarioCycleMovesToPublished();
        ScenarioCycleRejectsMovesFromRetired();
    }
}
