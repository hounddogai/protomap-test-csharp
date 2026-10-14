namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class PendingHolidayReservationViewChecks
{
    public static void PendingHolidayReservationViewShowsTheStatusInCapitals()
    {
        var item = PendingHolidayReservation.NewPendingHolidayReservation("code-689", "title-152");
        var view = PendingHolidayReservationView.ToPendingHolidayReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingHolidayReservationView.ToPendingHolidayReservationViews(new List<PendingHolidayReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingHolidayReservationViewShowsTheStatusInCapitals();
    }
}
