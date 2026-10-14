namespace Acme.Workforce.Orgdesign.Tests.Scenario.ScenarioAudit;

using Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

/// <summary>Checks of scenario audit code, run by the project's runner.</summary>
public static class LenientScenarioCycleChecks
{
    public static void LenientScenarioCycleStartsInIdle()
    {
        var item = LenientScenarioCycle.NewLenientScenarioCycle("code-797", "label-826");
        Check.Equal(item.Status, LenientScenarioCycleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientScenarioCycleMovesToReady()
    {
        var item = LenientScenarioCycle.NewLenientScenarioCycle("code-797", "label-826");
        try
        {
            item.MoveTo(LenientScenarioCycleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientScenarioCycleStatus.Ready, "status");
    }

    public static void LenientScenarioCycleRejectsMovesFromStopped()
    {
        var item = LenientScenarioCycle.NewLenientScenarioCycle("code-797", "label-826");
        item.Status = LenientScenarioCycleStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientScenarioCycleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientScenarioCycleStartsInIdle();
        LenientScenarioCycleMovesToReady();
        LenientScenarioCycleRejectsMovesFromStopped();
    }
}
