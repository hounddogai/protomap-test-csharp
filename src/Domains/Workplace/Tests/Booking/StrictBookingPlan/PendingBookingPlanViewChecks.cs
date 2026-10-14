namespace Acme.Workforce.Workplace.Tests.Booking.StrictBookingPlan;

using Acme.Workforce.Workplace.Booking.StrictBookingPlan;

/// <summary>Checks of strict booking plan code, run by the project's runner.</summary>
public static class PendingBookingPlanViewChecks
{
    public static void PendingBookingPlanViewShowsTheStatusInCapitals()
    {
        var item = PendingBookingPlan.NewPendingBookingPlan("code-998", "label-164");
        var view = PendingBookingPlanView.ToPendingBookingPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingBookingPlanView.ToPendingBookingPlanViews(new List<PendingBookingPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingBookingPlanViewShowsTheStatusInCapitals();
    }
}
