namespace Acme.Workforce.Absence.Tests.Holiday.HolidaySchedule;

using Acme.Workforce.Absence.Holiday.HolidaySchedule;

/// <summary>Checks of holiday schedule code, run by the project's runner.</summary>
public static class LegacyHolidayScheduleChecks
{
    public static void LegacyHolidayScheduleStartsInRequested()
    {
        var item = LegacyHolidaySchedule.NewLegacyHolidaySchedule("code-283", "label-748");
        Check.Equal(item.Status, LegacyHolidayScheduleStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyHolidayScheduleMovesToGranted()
    {
        var item = LegacyHolidaySchedule.NewLegacyHolidaySchedule("code-283", "label-748");
        try
        {
            item.MoveTo(LegacyHolidayScheduleStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyHolidayScheduleStatus.Granted, "status");
    }

    public static void LegacyHolidayScheduleRejectsMovesFromExpired()
    {
        var item = LegacyHolidaySchedule.NewLegacyHolidaySchedule("code-283", "label-748");
        item.Status = LegacyHolidayScheduleStatus.Expired;
        Check.Fails(() => item.MoveTo(LegacyHolidayScheduleStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LegacyHolidayScheduleStartsInRequested();
        LegacyHolidayScheduleMovesToGranted();
        LegacyHolidayScheduleRejectsMovesFromExpired();
    }
}
