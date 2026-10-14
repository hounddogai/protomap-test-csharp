namespace Acme.Workforce.Payrollops.Tests.Garnishment.Mapping;

using Acme.Workforce.Payrollops.Garnishment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdaptiveGarnishmentAllocationViewChecks
{
    public static void AdaptiveGarnishmentAllocationViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveGarnishmentAllocation.NewAdaptiveGarnishmentAllocation("code-445", "title-727");
        var view = AdaptiveGarnishmentAllocationView.ToAdaptiveGarnishmentAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveGarnishmentAllocationView.ToAdaptiveGarnishmentAllocationViews(new List<AdaptiveGarnishmentAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveGarnishmentAllocationViewShowsTheStatusInCapitals();
    }
}
