namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class BatchedAuditSummaryChecks
{
    public static void BatchedAuditSummaryStartsInRequested()
    {
        var item = BatchedAuditSummary.NewBatchedAuditSummary("code-790", "label-893");
        Check.Equal(item.Status, BatchedAuditSummaryStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAuditSummaryMovesToRevoked()
    {
        var item = BatchedAuditSummary.NewBatchedAuditSummary("code-790", "label-893");
        try
        {
            item.MoveTo(BatchedAuditSummaryStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAuditSummaryStatus.Revoked, "status");
    }

    public static void BatchedAuditSummaryRejectsMovesFromExpired()
    {
        var item = BatchedAuditSummary.NewBatchedAuditSummary("code-790", "label-893");
        item.Status = BatchedAuditSummaryStatus.Expired;
        Check.Fails(() => item.MoveTo(BatchedAuditSummaryStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BatchedAuditSummaryStartsInRequested();
        BatchedAuditSummaryMovesToRevoked();
        BatchedAuditSummaryRejectsMovesFromExpired();
    }
}
