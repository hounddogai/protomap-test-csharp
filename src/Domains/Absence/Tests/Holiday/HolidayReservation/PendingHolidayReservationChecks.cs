namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class PendingHolidayReservationChecks
{
    public static void PendingHolidayReservationStartsInProposed()
    {
        var item = PendingHolidayReservation.NewPendingHolidayReservation("code-689", "title-152");
        Check.Equal(item.Status, PendingHolidayReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingHolidayReservationMovesToApproved()
    {
        var item = PendingHolidayReservation.NewPendingHolidayReservation("code-689", "title-152");
        try
        {
            item.MoveTo(PendingHolidayReservationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingHolidayReservationStatus.Approved, "status");
    }

    public static void PendingHolidayReservationRejectsMovesFromWithdrawn()
    {
        var item = PendingHolidayReservation.NewPendingHolidayReservation("code-689", "title-152");
        item.Status = PendingHolidayReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingHolidayReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingHolidayReservationStartsInProposed();
        PendingHolidayReservationMovesToApproved();
        PendingHolidayReservationRejectsMovesFromWithdrawn();
    }
}
