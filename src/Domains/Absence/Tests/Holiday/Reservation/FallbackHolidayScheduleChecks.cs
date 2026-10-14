namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FallbackHolidayScheduleChecks
{
    public static void FallbackHolidayScheduleStartsInProposed()
    {
        var item = FallbackHolidaySchedule.NewFallbackHolidaySchedule("code-850", "title-628");
        Check.Equal(item.Status, FallbackHolidayScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackHolidayScheduleMovesToRejected()
    {
        var item = FallbackHolidaySchedule.NewFallbackHolidaySchedule("code-850", "title-628");
        try
        {
            item.MoveTo(FallbackHolidayScheduleStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackHolidayScheduleStatus.Rejected, "status");
    }

    public static void FallbackHolidayScheduleRejectsMovesFromWithdrawn()
    {
        var item = FallbackHolidaySchedule.NewFallbackHolidaySchedule("code-850", "title-628");
        item.Status = FallbackHolidayScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackHolidayScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackHolidayScheduleStartsInProposed();
        FallbackHolidayScheduleMovesToRejected();
        FallbackHolidayScheduleRejectsMovesFromWithdrawn();
    }
}
