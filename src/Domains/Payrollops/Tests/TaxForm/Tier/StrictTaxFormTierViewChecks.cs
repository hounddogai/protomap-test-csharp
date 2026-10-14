namespace Acme.Workforce.Payrollops.Tests.TaxForm.Tier;

using Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class StrictTaxFormTierViewChecks
{
    public static void StrictTaxFormTierViewShowsTheStatusInCapitals()
    {
        var item = StrictTaxFormTier.NewStrictTaxFormTier("code-913", "label-939");
        var view = StrictTaxFormTierView.ToStrictTaxFormTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictTaxFormTierView.ToStrictTaxFormTierViews(new List<StrictTaxFormTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictTaxFormTierViewShowsTheStatusInCapitals();
    }
}
