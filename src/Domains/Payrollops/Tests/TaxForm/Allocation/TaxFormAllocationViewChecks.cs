namespace Acme.Workforce.Payrollops.Tests.TaxForm.Allocation;

using Acme.Workforce.Payrollops.TaxForm.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class TaxFormAllocationViewChecks
{
    public static void TaxFormAllocationViewShowsTheStatusInCapitals()
    {
        var item = TaxFormAllocation.NewTaxFormAllocation("code-400", "label-623");
        var view = TaxFormAllocationView.ToTaxFormAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TaxFormAllocationView.ToTaxFormAllocationViews(new List<TaxFormAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TaxFormAllocationViewShowsTheStatusInCapitals();
    }
}
