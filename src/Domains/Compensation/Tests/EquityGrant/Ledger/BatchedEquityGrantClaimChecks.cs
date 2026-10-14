namespace Acme.Workforce.Compensation.Tests.EquityGrant.Ledger;

using Acme.Workforce.Compensation.EquityGrant.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BatchedEquityGrantClaimChecks
{
    public static void BatchedEquityGrantClaimStartsInNew()
    {
        var item = BatchedEquityGrantClaim.NewBatchedEquityGrantClaim("code-160", "title-31");
        Check.Equal(item.Status, BatchedEquityGrantClaimStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedEquityGrantClaimMovesToRunning()
    {
        var item = BatchedEquityGrantClaim.NewBatchedEquityGrantClaim("code-160", "title-31");
        try
        {
            item.MoveTo(BatchedEquityGrantClaimStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedEquityGrantClaimStatus.Running, "status");
    }

    public static void BatchedEquityGrantClaimRejectsMovesFromFailed()
    {
        var item = BatchedEquityGrantClaim.NewBatchedEquityGrantClaim("code-160", "title-31");
        item.Status = BatchedEquityGrantClaimStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedEquityGrantClaimStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedEquityGrantClaimStartsInNew();
        BatchedEquityGrantClaimMovesToRunning();
        BatchedEquityGrantClaimRejectsMovesFromFailed();
    }
}
