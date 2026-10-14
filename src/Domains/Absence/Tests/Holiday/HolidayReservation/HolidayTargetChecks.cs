namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class HolidayTargetChecks
{
    public static void HolidayTargetStartsInIdle()
    {
        var item = HolidayTarget.NewHolidayTarget("code-221", "name-393");
        Check.Equal(item.Status, HolidayTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayTargetMovesToReady()
    {
        var item = HolidayTarget.NewHolidayTarget("code-221", "name-393");
        try
        {
            item.MoveTo(HolidayTargetStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayTargetStatus.Ready, "status");
    }

    public static void HolidayTargetRejectsMovesFromStopped()
    {
        var item = HolidayTarget.NewHolidayTarget("code-221", "name-393");
        item.Status = HolidayTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(HolidayTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HolidayTargetStartsInIdle();
        HolidayTargetMovesToReady();
        HolidayTargetRejectsMovesFromStopped();
    }
}
