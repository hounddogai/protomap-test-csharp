namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class DefaultScenarioWindowChecks
{
    public static void DefaultScenarioWindowStartsInNew()
    {
        var item = DefaultScenarioWindow.NewDefaultScenarioWindow("code-439", "label-870");
        Check.Equal(item.Status, DefaultScenarioWindowStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultScenarioWindowMovesToRunning()
    {
        var item = DefaultScenarioWindow.NewDefaultScenarioWindow("code-439", "label-870");
        try
        {
            item.MoveTo(DefaultScenarioWindowStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultScenarioWindowStatus.Running, "status");
    }

    public static void DefaultScenarioWindowRejectsMovesFromFailed()
    {
        var item = DefaultScenarioWindow.NewDefaultScenarioWindow("code-439", "label-870");
        item.Status = DefaultScenarioWindowStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultScenarioWindowStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultScenarioWindowStartsInNew();
        DefaultScenarioWindowMovesToRunning();
        DefaultScenarioWindowRejectsMovesFromFailed();
    }
}
