namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class HolidayReservationViewChecks
{
    public static void HolidayReservationViewShowsTheStatusInCapitals()
    {
        var item = HolidayReservation.NewHolidayReservation("code-33", "name-220");
        var view = HolidayReservationView.ToHolidayReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HolidayReservationView.ToHolidayReservationViews(new List<HolidayReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HolidayReservationViewShowsTheStatusInCapitals();
    }
}
