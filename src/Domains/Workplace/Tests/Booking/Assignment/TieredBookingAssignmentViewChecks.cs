namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredBookingAssignmentViewChecks
{
    public static void TieredBookingAssignmentViewShowsTheStatusInCapitals()
    {
        var item = TieredBookingAssignment.NewTieredBookingAssignment("code-308", "label-908");
        var view = TieredBookingAssignmentView.ToTieredBookingAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredBookingAssignmentView.ToTieredBookingAssignmentViews(new List<TieredBookingAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredBookingAssignmentViewShowsTheStatusInCapitals();
    }
}
