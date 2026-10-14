namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class BatchedAuditTemplateViewChecks
{
    public static void BatchedAuditTemplateViewShowsTheStatusInCapitals()
    {
        var item = BatchedAuditTemplate.NewBatchedAuditTemplate("code-305", "title-780");
        var view = BatchedAuditTemplateView.ToBatchedAuditTemplateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedAuditTemplateView.ToBatchedAuditTemplateViews(new List<BatchedAuditTemplate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedAuditTemplateViewShowsTheStatusInCapitals();
    }
}
