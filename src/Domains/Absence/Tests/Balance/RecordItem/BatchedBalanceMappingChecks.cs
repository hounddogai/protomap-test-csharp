namespace Acme.Workforce.Absence.Tests.Balance.RecordItem;

using Acme.Workforce.Absence.Balance.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedBalanceMappingChecks
{
    public static void BatchedBalanceMappingStartsInProposed()
    {
        var item = BatchedBalanceMapping.NewBatchedBalanceMapping("code-108", "name-719");
        Check.Equal(item.Status, BatchedBalanceMappingStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedBalanceMappingMovesToRejected()
    {
        var item = BatchedBalanceMapping.NewBatchedBalanceMapping("code-108", "name-719");
        try
        {
            item.MoveTo(BatchedBalanceMappingStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedBalanceMappingStatus.Rejected, "status");
    }

    public static void BatchedBalanceMappingRejectsMovesFromWithdrawn()
    {
        var item = BatchedBalanceMapping.NewBatchedBalanceMapping("code-108", "name-719");
        item.Status = BatchedBalanceMappingStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedBalanceMappingStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedBalanceMappingStartsInProposed();
        BatchedBalanceMappingMovesToRejected();
        BatchedBalanceMappingRejectsMovesFromWithdrawn();
    }
}
