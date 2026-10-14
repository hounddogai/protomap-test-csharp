namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class TieredReturnPlanThresholdChecks
{
    public static void TieredReturnPlanThresholdStartsInIdle()
    {
        var item = TieredReturnPlanThreshold.NewTieredReturnPlanThreshold("code-560", "label-678");
        Check.Equal(item.Status, TieredReturnPlanThresholdStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredReturnPlanThresholdMovesToReady()
    {
        var item = TieredReturnPlanThreshold.NewTieredReturnPlanThreshold("code-560", "label-678");
        try
        {
            item.MoveTo(TieredReturnPlanThresholdStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredReturnPlanThresholdStatus.Ready, "status");
    }

    public static void TieredReturnPlanThresholdRejectsMovesFromStopped()
    {
        var item = TieredReturnPlanThreshold.NewTieredReturnPlanThreshold("code-560", "label-678");
        item.Status = TieredReturnPlanThresholdStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredReturnPlanThresholdStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredReturnPlanThresholdStartsInIdle();
        TieredReturnPlanThresholdMovesToReady();
        TieredReturnPlanThresholdRejectsMovesFromStopped();
    }
}
