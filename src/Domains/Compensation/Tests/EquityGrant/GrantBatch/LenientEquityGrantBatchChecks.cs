namespace Acme.Workforce.Compensation.Tests.EquityGrant.GrantBatch;

using Acme.Workforce.Compensation.EquityGrant.GrantBatch;

/// <summary>Checks of grant batch code, run by the project's runner.</summary>
public static class LenientEquityGrantBatchChecks
{
    public static void LenientEquityGrantBatchStartsInProposed()
    {
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("code-59", "name-319");
        Check.Equal(item.Status, LenientEquityGrantBatchStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientEquityGrantBatchMovesToApproved()
    {
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("code-59", "name-319");
        try
        {
            item.MoveTo(LenientEquityGrantBatchStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientEquityGrantBatchStatus.Approved, "status");
    }

    public static void LenientEquityGrantBatchRejectsMovesFromWithdrawn()
    {
        var item = LenientEquityGrantBatch.NewLenientEquityGrantBatch("code-59", "name-319");
        item.Status = LenientEquityGrantBatchStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(LenientEquityGrantBatchStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        LenientEquityGrantBatchStartsInProposed();
        LenientEquityGrantBatchMovesToApproved();
        LenientEquityGrantBatchRejectsMovesFromWithdrawn();
    }
}
