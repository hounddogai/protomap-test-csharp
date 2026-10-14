namespace Acme.Workforce.Workplace.Tests.Booking.BookingPlan;

using Acme.Workforce.Workplace.Booking.BookingPlan;

/// <summary>Checks of booking plan code, run by the project's runner.</summary>
public static class LenientBookingPlanViewChecks
{
    public static void LenientBookingPlanViewShowsTheStatusInCapitals()
    {
        var item = LenientBookingPlan.NewLenientBookingPlan("code-644", "name-892");
        var view = LenientBookingPlanView.ToLenientBookingPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientBookingPlanView.ToLenientBookingPlanViews(new List<LenientBookingPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientBookingPlanViewShowsTheStatusInCapitals();
    }
}
