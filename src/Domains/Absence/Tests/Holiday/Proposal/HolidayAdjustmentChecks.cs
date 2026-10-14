namespace Acme.Workforce.Absence.Tests.Holiday.Proposal;

using Acme.Workforce.Absence.Holiday.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HolidayAdjustmentChecks
{
    public static void HolidayAdjustmentStartsInIdle()
    {
        var item = HolidayAdjustment.NewHolidayAdjustment("code-164", "label-458");
        Check.Equal(item.Status, HolidayAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayAdjustmentMovesToReady()
    {
        var item = HolidayAdjustment.NewHolidayAdjustment("code-164", "label-458");
        try
        {
            item.MoveTo(HolidayAdjustmentStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayAdjustmentStatus.Ready, "status");
    }

    public static void HolidayAdjustmentRejectsMovesFromStopped()
    {
        var item = HolidayAdjustment.NewHolidayAdjustment("code-164", "label-458");
        item.Status = HolidayAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(HolidayAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HolidayAdjustmentStartsInIdle();
        HolidayAdjustmentMovesToReady();
        HolidayAdjustmentRejectsMovesFromStopped();
    }
}
