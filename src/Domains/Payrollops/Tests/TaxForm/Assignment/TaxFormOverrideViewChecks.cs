namespace Acme.Workforce.Payrollops.Tests.TaxForm.Assignment;

using Acme.Workforce.Payrollops.TaxForm.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TaxFormOverrideViewChecks
{
    public static void TaxFormOverrideViewShowsTheStatusInCapitals()
    {
        var item = TaxFormOverride.NewTaxFormOverride("code-86", "title-537");
        var view = TaxFormOverrideView.ToTaxFormOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormOverrideView.ToTaxFormOverrideViews(new List<TaxFormOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormOverrideViewShowsTheStatusInCapitals();
    }
}
