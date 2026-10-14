namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioEstimate;

using Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

/// <summary>Checks of scenario estimate code, run by the project's runner.</summary>
public static class GlobalScenarioBundleChecks
{
    public static void GlobalScenarioBundleStartsInDraft()
    {
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("code-872", "label-866");
        Check.Equal(item.Status, GlobalScenarioBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalScenarioBundleMovesToClosed()
    {
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("code-872", "label-866");
        try
        {
            item.MoveTo(GlobalScenarioBundleStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalScenarioBundleStatus.Closed, "status");
    }

    public static void GlobalScenarioBundleRejectsMovesFromClosed()
    {
        var item = GlobalScenarioBundle.NewGlobalScenarioBundle("code-872", "label-866");
        item.Status = GlobalScenarioBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalScenarioBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalScenarioBundleStartsInDraft();
        GlobalScenarioBundleMovesToClosed();
        GlobalScenarioBundleRejectsMovesFromClosed();
    }
}
