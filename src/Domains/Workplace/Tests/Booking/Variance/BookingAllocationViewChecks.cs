namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingAllocationViewChecks
{
    public static void BookingAllocationViewShowsTheStatusInCapitals()
    {
        var item = BookingAllocation.NewBookingAllocation("code-536", "label-779");
        var view = BookingAllocationView.ToBookingAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingAllocationView.ToBookingAllocationViews(new List<BookingAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingAllocationViewShowsTheStatusInCapitals();
    }
}
