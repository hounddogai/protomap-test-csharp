namespace Acme.Workforce.Orgdesign.Tests.Scenario.Summary;

using Acme.Workforce.Orgdesign.Scenario.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScenarioThresholdChecks
{
    public static void ScenarioThresholdStartsInIdle()
    {
        var item = ScenarioThreshold.NewScenarioThreshold("code-922", "label-534");
        Check.Equal(item.Status, ScenarioThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScenarioThresholdMovesToReady()
    {
        var item = ScenarioThreshold.NewScenarioThreshold("code-922", "label-534");
        try
        {
            item.MoveTo(ScenarioThresholdStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScenarioThresholdStatus.Ready, "status");
    }

    public static void ScenarioThresholdRejectsMovesFromStopped()
    {
        var item = ScenarioThreshold.NewScenarioThreshold("code-922", "label-534");
        item.Status = ScenarioThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScenarioThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScenarioThresholdStartsInIdle();
        ScenarioThresholdMovesToReady();
        ScenarioThresholdRejectsMovesFromStopped();
    }
}
