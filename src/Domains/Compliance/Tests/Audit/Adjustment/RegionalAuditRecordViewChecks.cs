namespace Acme.Workforce.Compliance.Tests.Audit.Adjustment;

using Acme.Workforce.Compliance.Audit.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RegionalAuditRecordViewChecks
{
    public static void RegionalAuditRecordViewShowsTheStatusInCapitals()
    {
        var item = RegionalAuditRecord.NewRegionalAuditRecord("code-303", "label-653");
        var view = RegionalAuditRecordView.ToRegionalAuditRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalAuditRecordView.ToRegionalAuditRecordViews(new List<RegionalAuditRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalAuditRecordViewShowsTheStatusInCapitals();
    }
}
