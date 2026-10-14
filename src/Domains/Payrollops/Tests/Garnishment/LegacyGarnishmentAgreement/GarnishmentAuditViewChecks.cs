namespace Acme.Workforce.Payrollops.Tests.Garnishment.LegacyGarnishmentAgreement;

using Acme.Workforce.Payrollops.Garnishment.LegacyGarnishmentAgreement;

/// <summary>Checks of legacy garnishment agreement code, run by the project's runner.</summary>
public static class GarnishmentAuditViewChecks
{
    public static void GarnishmentAuditViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentAudit.NewGarnishmentAudit("code-587", "name-678");
        var view = GarnishmentAuditView.ToGarnishmentAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentAuditView.ToGarnishmentAuditViews(new List<GarnishmentAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentAuditViewShowsTheStatusInCapitals();
    }
}
