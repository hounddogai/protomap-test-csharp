namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AuditBatchChecks
{
    public static void AuditBatchStartsInOpen()
    {
        var item = AuditBatch.NewAuditBatch("code-490", "title-417");
        Check.Equal(item.Status, AuditBatchStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AuditBatchMovesToInReview()
    {
        var item = AuditBatch.NewAuditBatch("code-490", "title-417");
        try
        {
            item.MoveTo(AuditBatchStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AuditBatchStatus.InReview, "status");
    }

    public static void AuditBatchRejectsMovesFromArchived()
    {
        var item = AuditBatch.NewAuditBatch("code-490", "title-417");
        item.Status = AuditBatchStatus.Archived;
        Check.Fails(() => item.MoveTo(AuditBatchStatus.Open), "move");
    }

    public static void RunAll()
    {
        AuditBatchStartsInOpen();
        AuditBatchMovesToInReview();
        AuditBatchRejectsMovesFromArchived();
    }
}
