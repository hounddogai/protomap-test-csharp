namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class BatchedAuditTemplateChecks
{
    public static void BatchedAuditTemplateStartsInNew()
    {
        var item = BatchedAuditTemplate.NewBatchedAuditTemplate("code-305", "title-780");
        Check.Equal(item.Status, BatchedAuditTemplateStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedAuditTemplateMovesToRunning()
    {
        var item = BatchedAuditTemplate.NewBatchedAuditTemplate("code-305", "title-780");
        try
        {
            item.MoveTo(BatchedAuditTemplateStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedAuditTemplateStatus.Running, "status");
    }

    public static void BatchedAuditTemplateRejectsMovesFromFailed()
    {
        var item = BatchedAuditTemplate.NewBatchedAuditTemplate("code-305", "title-780");
        item.Status = BatchedAuditTemplateStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedAuditTemplateStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedAuditTemplateStartsInNew();
        BatchedAuditTemplateMovesToRunning();
        BatchedAuditTemplateRejectsMovesFromFailed();
    }
}
