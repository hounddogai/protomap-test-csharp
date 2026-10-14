namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class BatchedPoolAuditChecks
{
    public static void BatchedPoolAuditStartsInCreated()
    {
        var item = BatchedPoolAudit.NewBatchedPoolAudit("code-782", "name-650");
        Check.Equal(item.Status, BatchedPoolAuditStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPoolAuditMovesToRetired()
    {
        var item = BatchedPoolAudit.NewBatchedPoolAudit("code-782", "name-650");
        try
        {
            item.MoveTo(BatchedPoolAuditStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPoolAuditStatus.Retired, "status");
    }

    public static void BatchedPoolAuditRejectsMovesFromRetired()
    {
        var item = BatchedPoolAudit.NewBatchedPoolAudit("code-782", "name-650");
        item.Status = BatchedPoolAuditStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedPoolAuditStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedPoolAuditStartsInCreated();
        BatchedPoolAuditMovesToRetired();
        BatchedPoolAuditRejectsMovesFromRetired();
    }
}
