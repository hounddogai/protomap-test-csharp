namespace Acme.Workforce.Payrollops.Tests.TaxForm.FormEstimate;

using Acme.Workforce.Payrollops.TaxForm.FormEstimate;

/// <summary>Checks of form estimate code, run by the project's runner.</summary>
public static class SeasonalTaxFormTierViewChecks
{
    public static void SeasonalTaxFormTierViewShowsTheStatusInCapitals()
    {
        var item = SeasonalTaxFormTier.NewSeasonalTaxFormTier("code-126", "label-582");
        var view = SeasonalTaxFormTierView.ToSeasonalTaxFormTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalTaxFormTierView.ToSeasonalTaxFormTierViews(new List<SeasonalTaxFormTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalTaxFormTierViewShowsTheStatusInCapitals();
    }
}
