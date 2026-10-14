namespace Acme.Workforce.Compensation.Tests.Adjustment.Summary;

using Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BatchedAdjustmentClaimChecks
{
    public static void BatchedAdjustmentClaimStartsInCreated()
    {
        var item = BatchedAdjustmentClaim.NewBatchedAdjustmentClaim("code-929", "title-657");
        Check.Equal(item.Status, BatchedAdjustmentClaimStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAdjustmentClaimMovesToValidated()
    {
        var item = BatchedAdjustmentClaim.NewBatchedAdjustmentClaim("code-929", "title-657");
        try
        {
            item.MoveTo(BatchedAdjustmentClaimStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAdjustmentClaimStatus.Validated, "status");
    }

    public static void BatchedAdjustmentClaimRejectsMovesFromRetired()
    {
        var item = BatchedAdjustmentClaim.NewBatchedAdjustmentClaim("code-929", "title-657");
        item.Status = BatchedAdjustmentClaimStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedAdjustmentClaimStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedAdjustmentClaimStartsInCreated();
        BatchedAdjustmentClaimMovesToValidated();
        BatchedAdjustmentClaimRejectsMovesFromRetired();
    }
}
