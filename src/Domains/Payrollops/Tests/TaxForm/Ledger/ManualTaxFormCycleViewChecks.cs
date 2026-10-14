namespace Acme.Workforce.Payrollops.Tests.TaxForm.Ledger;

using Acme.Workforce.Payrollops.TaxForm.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ManualTaxFormCycleViewChecks
{
    public static void ManualTaxFormCycleViewShowsTheStatusInCapitals()
    {
        var item = ManualTaxFormCycle.NewManualTaxFormCycle("code-902", "name-723");
        var view = ManualTaxFormCycleView.ToManualTaxFormCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualTaxFormCycleView.ToManualTaxFormCycleViews(new List<ManualTaxFormCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualTaxFormCycleViewShowsTheStatusInCapitals();
    }
}
