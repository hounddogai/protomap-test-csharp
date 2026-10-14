namespace Acme.Workforce.Absence.Tests.Holiday.HolidaySchedule;

using Acme.Workforce.Absence.Holiday.HolidaySchedule;

/// <summary>Checks of holiday schedule code, run by the project's runner.</summary>
public static class SeasonalHolidayAssignmentChecks
{
    public static void SeasonalHolidayAssignmentStartsInOpen()
    {
        var item = SeasonalHolidayAssignment.NewSeasonalHolidayAssignment("code-774", "label-444");
        Check.Equal(item.Status, SeasonalHolidayAssignmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalHolidayAssignmentMovesToResolved()
    {
        var item = SeasonalHolidayAssignment.NewSeasonalHolidayAssignment("code-774", "label-444");
        try
        {
            item.MoveTo(SeasonalHolidayAssignmentStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalHolidayAssignmentStatus.Resolved, "status");
    }

    public static void SeasonalHolidayAssignmentRejectsMovesFromArchived()
    {
        var item = SeasonalHolidayAssignment.NewSeasonalHolidayAssignment("code-774", "label-444");
        item.Status = SeasonalHolidayAssignmentStatus.Archived;
        Check.Fails(() => item.MoveTo(SeasonalHolidayAssignmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        SeasonalHolidayAssignmentStartsInOpen();
        SeasonalHolidayAssignmentMovesToResolved();
        SeasonalHolidayAssignmentRejectsMovesFromArchived();
    }
}
