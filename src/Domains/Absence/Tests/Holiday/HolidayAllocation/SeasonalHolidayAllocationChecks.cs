namespace Acme.Workforce.Absence.Tests.Holiday.HolidayAllocation;

using Acme.Workforce.Absence.Holiday.HolidayAllocation;

/// <summary>Checks of holiday allocation code, run by the project's runner.</summary>
public static class SeasonalHolidayAllocationChecks
{
    public static void SeasonalHolidayAllocationStartsInOpen()
    {
        var item = SeasonalHolidayAllocation.NewSeasonalHolidayAllocation("code-541", "name-720");
        Check.Equal(item.Status, SeasonalHolidayAllocationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalHolidayAllocationMovesToInReview()
    {
        var item = SeasonalHolidayAllocation.NewSeasonalHolidayAllocation("code-541", "name-720");
        try
        {
            item.MoveTo(SeasonalHolidayAllocationStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalHolidayAllocationStatus.InReview, "status");
    }

    public static void SeasonalHolidayAllocationRejectsMovesFromArchived()
    {
        var item = SeasonalHolidayAllocation.NewSeasonalHolidayAllocation("code-541", "name-720");
        item.Status = SeasonalHolidayAllocationStatus.Archived;
        Check.Fails(() => item.MoveTo(SeasonalHolidayAllocationStatus.Open), "move");
    }

    public static void RunAll()
    {
        SeasonalHolidayAllocationStartsInOpen();
        SeasonalHolidayAllocationMovesToInReview();
        SeasonalHolidayAllocationRejectsMovesFromArchived();
    }
}
