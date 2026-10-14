namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AuditBatchViewChecks
{
    public static void AuditBatchViewShowsTheStatusInCapitals()
    {
        var item = AuditBatch.NewAuditBatch("code-490", "title-417");
        var view = AuditBatchView.ToAuditBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditBatchView.ToAuditBatchViews(new List<AuditBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditBatchViewShowsTheStatusInCapitals();
    }
}
