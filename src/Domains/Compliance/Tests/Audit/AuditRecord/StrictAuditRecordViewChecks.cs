namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class StrictAuditRecordViewChecks
{
    public static void StrictAuditRecordViewShowsTheStatusInCapitals()
    {
        var item = StrictAuditRecord.NewStrictAuditRecord("code-591", "title-863");
        var view = StrictAuditRecordView.ToStrictAuditRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictAuditRecordView.ToStrictAuditRecordViews(new List<StrictAuditRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictAuditRecordViewShowsTheStatusInCapitals();
    }
}
