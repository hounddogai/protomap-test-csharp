namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class BatchedAuditCycleChecks
{
    public static void BatchedAuditCycleStartsInCreated()
    {
        var item = BatchedAuditCycle.NewBatchedAuditCycle("code-221", "name-19");
        Check.Equal(item.Status, BatchedAuditCycleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAuditCycleMovesToValidated()
    {
        var item = BatchedAuditCycle.NewBatchedAuditCycle("code-221", "name-19");
        try
        {
            item.MoveTo(BatchedAuditCycleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAuditCycleStatus.Validated, "status");
    }

    public static void BatchedAuditCycleRejectsMovesFromRetired()
    {
        var item = BatchedAuditCycle.NewBatchedAuditCycle("code-221", "name-19");
        item.Status = BatchedAuditCycleStatus.Retired;
        Check.Fails(() => item.MoveTo(BatchedAuditCycleStatus.Created), "move");
    }

    public static void RunAll()
    {
        BatchedAuditCycleStartsInCreated();
        BatchedAuditCycleMovesToValidated();
        BatchedAuditCycleRejectsMovesFromRetired();
    }
}
