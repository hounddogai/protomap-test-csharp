namespace Acme.Workforce.Payrollops.Tests.TaxForm.Rollup;

using Acme.Workforce.Payrollops.TaxForm.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TaxFormBaselineViewChecks
{
    public static void TaxFormBaselineViewShowsTheStatusInCapitals()
    {
        var item = TaxFormBaseline.NewTaxFormBaseline("code-209", "title-901");
        var view = TaxFormBaselineView.ToTaxFormBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormBaselineView.ToTaxFormBaselineViews(new List<TaxFormBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormBaselineViewShowsTheStatusInCapitals();
    }
}
