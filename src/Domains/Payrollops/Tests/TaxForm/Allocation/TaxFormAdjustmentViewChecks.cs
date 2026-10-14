namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAdjustmentViewChecks
{
    public static void TaxFormAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = TaxFormAdjustment.NewTaxFormAdjustment("code-803", "label-588");
        var view = TaxFormAdjustmentView.ToTaxFormAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormAdjustmentView.ToTaxFormAdjustmentViews(new List<TaxFormAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormAdjustmentViewShowsTheStatusInCapitals();
    }
}
