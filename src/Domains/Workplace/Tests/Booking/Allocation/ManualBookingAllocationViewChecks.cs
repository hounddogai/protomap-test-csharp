namespace Acme.Workforce.Workplace.Tests.Booking.Allocation;

using Acme.Workforce.Workplace.Booking.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class ManualBookingAllocationViewChecks
{
    public static void ManualBookingAllocationViewShowsTheStatusInCapitals()
    {
        var item = ManualBookingAllocation.NewManualBookingAllocation("code-753", "label-243");
        var view = ManualBookingAllocationView.ToManualBookingAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualBookingAllocationView.ToManualBookingAllocationViews(new List<ManualBookingAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualBookingAllocationViewShowsTheStatusInCapitals();
    }
}
