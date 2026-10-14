namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedAdjustmentRuleChecks
{
    public static void BatchedAdjustmentRuleStartsInRequested()
    {
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("code-196", "title-555");
        Check.Equal(item.Status, BatchedAdjustmentRuleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAdjustmentRuleMovesToRevoked()
    {
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("code-196", "title-555");
        try
        {
            item.MoveTo(BatchedAdjustmentRuleStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAdjustmentRuleStatus.Revoked, "status");
    }

    public static void BatchedAdjustmentRuleRejectsMovesFromExpired()
    {
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("code-196", "title-555");
        item.Status = BatchedAdjustmentRuleStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedAdjustmentRuleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedAdjustmentRuleStartsInRequested();
        BatchedAdjustmentRuleMovesToRevoked();
        BatchedAdjustmentRuleRejectsMovesFromExpired();
    }
}
