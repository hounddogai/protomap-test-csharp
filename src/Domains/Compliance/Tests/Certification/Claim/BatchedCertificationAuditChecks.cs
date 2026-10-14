namespace Acme.Workforce.Compliance.Tests.Certification.Claim;

using Acme.Workforce.Compliance.Certification.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class BatchedCertificationAuditChecks
{
    public static void BatchedCertificationAuditStartsInNew()
    {
        var item = BatchedCertificationAudit.NewBatchedCertificationAudit("code-972", "name-418");
        Check.Equal(item.Status, BatchedCertificationAuditStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedCertificationAuditMovesToQueued()
    {
        var item = BatchedCertificationAudit.NewBatchedCertificationAudit("code-972", "name-418");
        try
        {
            item.MoveTo(BatchedCertificationAuditStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedCertificationAuditStatus.Queued, "status");
    }

    public static void BatchedCertificationAuditRejectsMovesFromFailed()
    {
        var item = BatchedCertificationAudit.NewBatchedCertificationAudit("code-972", "name-418");
        item.Status = BatchedCertificationAuditStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedCertificationAuditStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedCertificationAuditStartsInNew();
        BatchedCertificationAuditMovesToQueued();
        BatchedCertificationAuditRejectsMovesFromFailed();
    }
}
