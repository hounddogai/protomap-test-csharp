namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAllocation;

/// <summary>Checks of garnishment allocation code, run by the project's runner.</summary>
public static class LegacyGarnishmentAllocationViewChecks
{
    public static void LegacyGarnishmentAllocationViewShowsTheStatusInCapitals()
    {
        var item = LegacyGarnishmentAllocation.NewLegacyGarnishmentAllocation("code-324", "label-575");
        var view = LegacyGarnishmentAllocationView.ToLegacyGarnishmentAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyGarnishmentAllocationView.ToLegacyGarnishmentAllocationViews(new List<LegacyGarnishmentAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyGarnishmentAllocationViewShowsTheStatusInCapitals();
    }
}
