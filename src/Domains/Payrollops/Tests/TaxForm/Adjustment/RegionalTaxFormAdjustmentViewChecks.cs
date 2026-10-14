namespace Acme.Workforce.Payrollops.Tests.TaxForm.Adjustment;

using Acme.Workforce.Payrollops.TaxForm.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RegionalTaxFormAdjustmentViewChecks
{
    public static void RegionalTaxFormAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = RegionalTaxFormAdjustment.NewRegionalTaxFormAdjustment("code-137", "title-83");
        var view = RegionalTaxFormAdjustmentView.ToRegionalTaxFormAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalTaxFormAdjustmentView.ToRegionalTaxFormAdjustmentViews(new List<RegionalTaxFormAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalTaxFormAdjustmentViewShowsTheStatusInCapitals();
    }
}
