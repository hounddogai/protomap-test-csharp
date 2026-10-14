namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class RollingTaxFormAdjustmentViewChecks
{
    public static void RollingTaxFormAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = RollingTaxFormAdjustment.NewRollingTaxFormAdjustment("code-971", "name-158");
        var view = RollingTaxFormAdjustmentView.ToRollingTaxFormAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingTaxFormAdjustmentView.ToRollingTaxFormAdjustmentViews(new List<RollingTaxFormAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingTaxFormAdjustmentViewShowsTheStatusInCapitals();
    }
}
