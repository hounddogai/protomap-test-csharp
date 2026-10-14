namespace Acme.Workforce.Orgdesign.Tests.Scenario.Cycle;

using Acme.Workforce.Orgdesign.Scenario.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class ScenarioWindowChecks
{
    public static void ScenarioWindowStartsInOpen()
    {
        var item = ScenarioWindow.NewScenarioWindow("code-764", "label-111");
        Check.Equal(item.Status, ScenarioWindowStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioWindowMovesToResolved()
    {
        var item = ScenarioWindow.NewScenarioWindow("code-764", "label-111");
        try
        {
            item.MoveTo(ScenarioWindowStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioWindowStatus.Resolved, "status");
    }

    public static void ScenarioWindowRejectsMovesFromArchived()
    {
        var item = ScenarioWindow.NewScenarioWindow("code-764", "label-111");
        item.Status = ScenarioWindowStatus.Archived;
        Check.Fails(() => item.MoveTo(ScenarioWindowStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScenarioWindowStartsInOpen();
        ScenarioWindowMovesToResolved();
        ScenarioWindowRejectsMovesFromArchived();
    }
}
