namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormMetric;

using Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>Checks of form metric code, run by the project's runner.</summary>
public static class TaxFormClaimViewChecks
{
    public static void TaxFormClaimViewShowsTheStatusInCapitals()
    {
        var item = TaxFormClaim.NewTaxFormClaim("code-460", "name-312");
        var view = TaxFormClaimView.ToTaxFormClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormClaimView.ToTaxFormClaimViews(new List<TaxFormClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormClaimViewShowsTheStatusInCapitals();
    }
}
