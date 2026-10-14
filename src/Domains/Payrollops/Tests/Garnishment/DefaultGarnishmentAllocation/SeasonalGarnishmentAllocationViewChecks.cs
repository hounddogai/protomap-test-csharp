namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class SeasonalGarnishmentAllocationViewChecks
{
    public static void SeasonalGarnishmentAllocationViewShowsTheStatusInCapitals()
    {
        var item = SeasonalGarnishmentAllocation.NewSeasonalGarnishmentAllocation("code-406", "title-465");
        var view = SeasonalGarnishmentAllocationView.ToSeasonalGarnishmentAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalGarnishmentAllocationView.ToSeasonalGarnishmentAllocationViews(new List<SeasonalGarnishmentAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalGarnishmentAllocationViewShowsTheStatusInCapitals();
    }
}
