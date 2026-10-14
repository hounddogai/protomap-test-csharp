namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class HolidayReservationChecks
{
    public static void HolidayReservationStartsInRequested()
    {
        var item = HolidayReservation.NewHolidayReservation("code-33", "name-220");
        Check.Equal(item.Status, HolidayReservationStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayReservationMovesToExpired()
    {
        var item = HolidayReservation.NewHolidayReservation("code-33", "name-220");
        try
        {
            item.MoveTo(HolidayReservationStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayReservationStatus.Expired, "status");
    }

    public static void HolidayReservationRejectsMovesFromExpired()
    {
        var item = HolidayReservation.NewHolidayReservation("code-33", "name-220");
        item.Status = HolidayReservationStatus.Expired;
        Check.Fails(() => item.MoveTo(HolidayReservationStatus.Requested), "move");
    }

    public static void RunAll()
    {
        HolidayReservationStartsInRequested();
        HolidayReservationMovesToExpired();
        HolidayReservationRejectsMovesFromExpired();
    }
}
