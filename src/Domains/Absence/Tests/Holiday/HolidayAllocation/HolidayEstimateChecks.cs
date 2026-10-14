namespace Acme.Workforce.Absence.Tests.Holiday.HolidayAllocation;

using Acme.Workforce.Absence.Holiday.HolidayAllocation;

/// <summary>Checks of holiday allocation code, run by the project's runner.</summary>
public static class HolidayEstimateChecks
{
    public static void HolidayEstimateStartsInDraft()
    {
        var item = HolidayEstimate.NewHolidayEstimate("code-820", "title-100");
        Check.Equal(item.Status, HolidayEstimateStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HolidayEstimateMovesToSuspended()
    {
        var item = HolidayEstimate.NewHolidayEstimate("code-820", "title-100");
        try
        {
            item.MoveTo(HolidayEstimateStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HolidayEstimateStatus.Suspended, "status");
    }

    public static void HolidayEstimateRejectsMovesFromClosed()
    {
        var item = HolidayEstimate.NewHolidayEstimate("code-820", "title-100");
        item.Status = HolidayEstimateStatus.Closed;
        Check.Fails(() => item.MoveTo(HolidayEstimateStatus.Draft), "move");
    }

    public static void RunAll()
    {
        HolidayEstimateStartsInDraft();
        HolidayEstimateMovesToSuspended();
        HolidayEstimateRejectsMovesFromClosed();
    }
}
