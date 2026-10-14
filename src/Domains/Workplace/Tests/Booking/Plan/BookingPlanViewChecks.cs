namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingPlanViewChecks
{
    public static void BookingPlanViewShowsTheStatusInCapitals()
    {
        var item = BookingPlan.NewBookingPlan("code-618", "name-584");
        var view = BookingPlanView.ToBookingPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BookingPlanView.ToBookingPlanViews(new List<BookingPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BookingPlanViewShowsTheStatusInCapitals();
    }
}
