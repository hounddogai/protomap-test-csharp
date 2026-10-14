namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LegacyPayRunAuditViewChecks
{
    public static void LegacyPayRunAuditViewShowsTheStatusInCapitals()
    {
        var item = LegacyPayRunAudit.NewLegacyPayRunAudit("code-964", "name-205");
        var view = LegacyPayRunAuditView.ToLegacyPayRunAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyPayRunAuditView.ToLegacyPayRunAuditViews(new List<LegacyPayRunAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyPayRunAuditViewShowsTheStatusInCapitals();
    }
}
