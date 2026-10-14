namespace Acme.Workforce.Absence.Tests.Holiday.Variance;

using Acme.Workforce.Absence.Holiday.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class HolidayThresholdChecks
{
    public static void HolidayThresholdStartsInPlanned()
    {
        var item = HolidayThreshold.NewHolidayThreshold("code-315", "title-572");
        Check.Equal(item.Status, HolidayThresholdStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayThresholdMovesToScheduled()
    {
        var item = HolidayThreshold.NewHolidayThreshold("code-315", "title-572");
        try
        {
            item.MoveTo(HolidayThresholdStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayThresholdStatus.Scheduled, "status");
    }

    public static void HolidayThresholdRejectsMovesFromCancelled()
    {
        var item = HolidayThreshold.NewHolidayThreshold("code-315", "title-572");
        item.Status = HolidayThresholdStatus.Cancelled;
        Check.Fails(() => item.MoveTo(HolidayThresholdStatus.Planned), "move");
    }

    public static void RunAll()
    {
        HolidayThresholdStartsInPlanned();
        HolidayThresholdMovesToScheduled();
        HolidayThresholdRejectsMovesFromCancelled();
    }
}
