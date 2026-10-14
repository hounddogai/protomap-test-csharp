namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AuditRecordViewChecks
{
    public static void AuditRecordViewShowsTheStatusInCapitals()
    {
        var item = AuditRecord.NewAuditRecord("code-882", "label-806");
        var view = AuditRecordView.ToAuditRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AuditRecordView.ToAuditRecordViews(new List<AuditRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AuditRecordViewShowsTheStatusInCapitals();
    }
}
