namespace Acme.Workforce.Payrollops.Tests.Garnishment.Allocation;

using Acme.Workforce.Payrollops.Garnishment.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class GarnishmentAllocationViewChecks
{
    public static void GarnishmentAllocationViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentAllocation.NewGarnishmentAllocation("code-196", "title-532");
        var view = GarnishmentAllocationView.ToGarnishmentAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentAllocationView.ToGarnishmentAllocationViews(new List<GarnishmentAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentAllocationViewShowsTheStatusInCapitals();
    }
}
