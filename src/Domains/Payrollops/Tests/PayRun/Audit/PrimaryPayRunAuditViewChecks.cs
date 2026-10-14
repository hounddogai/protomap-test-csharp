namespace Acme.Workforce.Payrollops.Tests.PayRun.Audit;

using Acme.Workforce.Payrollops.PayRun.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PrimaryPayRunAuditViewChecks
{
    public static void PrimaryPayRunAuditViewShowsTheStatusInCapitals()
    {
        var item = PrimaryPayRunAudit.NewPrimaryPayRunAudit("code-10", "title-564");
        var view = PrimaryPayRunAuditView.ToPrimaryPayRunAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryPayRunAuditView.ToPrimaryPayRunAuditViews(new List<PrimaryPayRunAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryPayRunAuditViewShowsTheStatusInCapitals();
    }
}
