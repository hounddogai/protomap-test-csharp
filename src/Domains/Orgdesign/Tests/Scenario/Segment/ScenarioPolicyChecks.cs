namespace Acme.Workforce.Orgdesign.Tests.Scenario.Segment;

using Acme.Workforce.Orgdesign.Scenario.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ScenarioPolicyChecks
{
    public static void ScenarioPolicyStartsInIdle()
    {
        var item = ScenarioPolicy.NewScenarioPolicy("code-920", "name-821");
        Check.Equal(item.Status, ScenarioPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioPolicyMovesToStopped()
    {
        var item = ScenarioPolicy.NewScenarioPolicy("code-920", "name-821");
        try
        {
            item.MoveTo(ScenarioPolicyStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioPolicyStatus.Stopped, "status");
    }

    public static void ScenarioPolicyRejectsMovesFromStopped()
    {
        var item = ScenarioPolicy.NewScenarioPolicy("code-920", "name-821");
        item.Status = ScenarioPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScenarioPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScenarioPolicyStartsInIdle();
        ScenarioPolicyMovesToStopped();
        ScenarioPolicyRejectsMovesFromStopped();
    }
}
