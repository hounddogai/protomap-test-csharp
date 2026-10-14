namespace Acme.Workforce.Payrollops.Tests.TaxForm.Template;

using Acme.Workforce.Payrollops.TaxForm.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class TaxFormVarianceViewChecks
{
    public static void TaxFormVarianceViewShowsTheStatusInCapitals()
    {
        var item = TaxFormVariance.NewTaxFormVariance("code-21", "label-217");
        var view = TaxFormVarianceView.ToTaxFormVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormVarianceView.ToTaxFormVarianceViews(new List<TaxFormVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormVarianceViewShowsTheStatusInCapitals();
    }
}
