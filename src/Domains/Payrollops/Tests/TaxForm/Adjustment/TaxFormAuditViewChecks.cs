namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TaxFormAuditViewChecks
{
    public static void TaxFormAuditViewShowsTheStatusInCapitals()
    {
        var item = TaxFormAudit.NewTaxFormAudit("code-560", "title-721");
        var view = TaxFormAuditView.ToTaxFormAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormAuditView.ToTaxFormAuditViews(new List<TaxFormAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormAuditViewShowsTheStatusInCapitals();
    }
}
