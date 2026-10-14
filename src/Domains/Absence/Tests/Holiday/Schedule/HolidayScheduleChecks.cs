namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class HolidayScheduleChecks
{
    public static void HolidayScheduleStartsInProposed()
    {
        var item = HolidaySchedule.NewHolidaySchedule("code-323", "title-496");
        Check.Equal(item.Status, HolidayScheduleStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayScheduleMovesToApproved()
    {
        var item = HolidaySchedule.NewHolidaySchedule("code-323", "title-496");
        try
        {
            item.MoveTo(HolidayScheduleStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayScheduleStatus.Approved, "status");
    }

    public static void HolidayScheduleRejectsMovesFromWithdrawn()
    {
        var item = HolidaySchedule.NewHolidaySchedule("code-323", "title-496");
        item.Status = HolidayScheduleStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(HolidayScheduleStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        HolidayScheduleStartsInProposed();
        HolidayScheduleMovesToApproved();
        HolidayScheduleRejectsMovesFromWithdrawn();
    }
}
