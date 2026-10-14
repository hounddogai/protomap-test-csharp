namespace Acme.Workforce.Orgdesign.Tests.Scenario.Request;

using Acme.Workforce.Orgdesign.Scenario.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class GlobalScenarioRequestChecks
{
    public static void GlobalScenarioRequestStartsInRequested()
    {
        var item = GlobalScenarioRequest.NewGlobalScenarioRequest("code-39", "title-655");
        Check.Equal(item.Status, GlobalScenarioRequestStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalScenarioRequestMovesToGranted()
    {
        var item = GlobalScenarioRequest.NewGlobalScenarioRequest("code-39", "title-655");
        try
        {
            item.MoveTo(GlobalScenarioRequestStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalScenarioRequestStatus.Granted, "status");
    }

    public static void GlobalScenarioRequestRejectsMovesFromExpired()
    {
        var item = GlobalScenarioRequest.NewGlobalScenarioRequest("code-39", "title-655");
        item.Status = GlobalScenarioRequestStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalScenarioRequestStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalScenarioRequestStartsInRequested();
        GlobalScenarioRequestMovesToGranted();
        GlobalScenarioRequestRejectsMovesFromExpired();
    }
}
