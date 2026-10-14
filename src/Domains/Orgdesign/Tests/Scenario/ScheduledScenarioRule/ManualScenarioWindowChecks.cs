namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScheduledScenarioRule;

using Acme.Workforce.Orgdesign.Scenario.ScheduledScenarioRule;

/// <summary>Checks of scheduled scenario rule code, run by the project's runner.</summary>
public static class ManualScenarioWindowChecks
{
    public static void ManualScenarioWindowStartsInRequested()
    {
        var item = ManualScenarioWindow.NewManualScenarioWindow("code-613", "title-689");
        Check.Equal(item.Status, ManualScenarioWindowStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualScenarioWindowMovesToGranted()
    {
        var item = ManualScenarioWindow.NewManualScenarioWindow("code-613", "title-689");
        try
        {
            item.MoveTo(ManualScenarioWindowStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualScenarioWindowStatus.Granted, "status");
    }

    public static void ManualScenarioWindowRejectsMovesFromExpired()
    {
        var item = ManualScenarioWindow.NewManualScenarioWindow("code-613", "title-689");
        item.Status = ManualScenarioWindowStatus.Expired;
        Check.Fails(() => item.MoveTo(ManualScenarioWindowStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ManualScenarioWindowStartsInRequested();
        ManualScenarioWindowMovesToGranted();
        ManualScenarioWindowRejectsMovesFromExpired();
    }
}
