namespace Acme.Workforce.Contractors.Tests.Rate.Slot;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class ScheduledRateAllocationViewChecks
{
    public static void ScheduledRateAllocationViewShowsTheStatusInCapitals()
    {
        var item = ScheduledRateAllocation.NewScheduledRateAllocation("code-638", "label-390");
        var view = ScheduledRateAllocationView.ToScheduledRateAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledRateAllocationView.ToScheduledRateAllocationViews(new List<ScheduledRateAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledRateAllocationViewShowsTheStatusInCapitals();
    }
}
