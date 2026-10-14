namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class ManualAdjustmentRuleChecks
{
    public static void ManualAdjustmentRuleStartsInIdle()
    {
        var item = ManualAdjustmentRule.NewManualAdjustmentRule("code-770", "title-367");
        Check.Equal(item.Status, ManualAdjustmentRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualAdjustmentRuleMovesToReady()
    {
        var item = ManualAdjustmentRule.NewManualAdjustmentRule("code-770", "title-367");
        try
        {
            item.MoveTo(ManualAdjustmentRuleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualAdjustmentRuleStatus.Ready, "status");
    }

    public static void ManualAdjustmentRuleRejectsMovesFromStopped()
    {
        var item = ManualAdjustmentRule.NewManualAdjustmentRule("code-770", "title-367");
        item.Status = ManualAdjustmentRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualAdjustmentRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualAdjustmentRuleStartsInIdle();
        ManualAdjustmentRuleMovesToReady();
        ManualAdjustmentRuleRejectsMovesFromStopped();
    }
}
