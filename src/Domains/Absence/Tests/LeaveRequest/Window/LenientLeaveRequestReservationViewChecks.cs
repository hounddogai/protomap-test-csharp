namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class LenientLeaveRequestReservationViewChecks
{
    public static void LenientLeaveRequestReservationViewShowsTheStatusInCapitals()
    {
        var item = LenientLeaveRequestReservation.NewLenientLeaveRequestReservation("code-456", "title-612");
        var view = LenientLeaveRequestReservationView.ToLenientLeaveRequestReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientLeaveRequestReservationView.ToLenientLeaveRequestReservationViews(new List<LenientLeaveRequestReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientLeaveRequestReservationViewShowsTheStatusInCapitals();
    }
}
