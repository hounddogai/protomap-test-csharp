namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedAdjustmentTierChecks
{
    public static void BatchedAdjustmentTierStartsInOpen()
    {
        var item = BatchedAdjustmentTier.NewBatchedAdjustmentTier("code-619", "title-439");
        Check.Equal(item.Status, BatchedAdjustmentTierStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAdjustmentTierMovesToResolved()
    {
        var item = BatchedAdjustmentTier.NewBatchedAdjustmentTier("code-619", "title-439");
        try
        {
            item.MoveTo(BatchedAdjustmentTierStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAdjustmentTierStatus.Resolved, "status");
    }

    public static void BatchedAdjustmentTierRejectsMovesFromArchived()
    {
        var item = BatchedAdjustmentTier.NewBatchedAdjustmentTier("code-619", "title-439");
        item.Status = BatchedAdjustmentTierStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedAdjustmentTierStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedAdjustmentTierStartsInOpen();
        BatchedAdjustmentTierMovesToResolved();
        BatchedAdjustmentTierRejectsMovesFromArchived();
    }
}
