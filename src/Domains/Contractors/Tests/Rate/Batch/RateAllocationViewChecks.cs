namespace Acme.Workforce.Contractors.Tests.Rate.Batch;

using Acme.Workforce.Contractors.Rate.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class RateAllocationViewChecks
{
    public static void RateAllocationViewShowsTheStatusInCapitals()
    {
        var item = RateAllocation.NewRateAllocation("code-947", "label-582");
        var view = RateAllocationView.ToRateAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RateAllocationView.ToRateAllocationViews(new List<RateAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RateAllocationViewShowsTheStatusInCapitals();
    }
}
