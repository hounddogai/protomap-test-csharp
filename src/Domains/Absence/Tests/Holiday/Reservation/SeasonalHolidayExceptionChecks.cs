namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SeasonalHolidayExceptionChecks
{
    public static void SeasonalHolidayExceptionStartsInOpen()
    {
        var item = SeasonalHolidayException.NewSeasonalHolidayException("code-528", "name-753");
        Check.Equal(item.Status, SeasonalHolidayExceptionStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalHolidayExceptionMovesToResolved()
    {
        var item = SeasonalHolidayException.NewSeasonalHolidayException("code-528", "name-753");
        try
        {
            item.MoveTo(SeasonalHolidayExceptionStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalHolidayExceptionStatus.Resolved, "status");
    }

    public static void SeasonalHolidayExceptionRejectsMovesFromArchived()
    {
        var item = SeasonalHolidayException.NewSeasonalHolidayException("code-528", "name-753");
        item.Status = SeasonalHolidayExceptionStatus.Archived;
        Check.Fails(() => item.MoveTo(SeasonalHolidayExceptionStatus.Open), "move");
    }

    public static void RunAll()
    {
        SeasonalHolidayExceptionStartsInOpen();
        SeasonalHolidayExceptionMovesToResolved();
        SeasonalHolidayExceptionRejectsMovesFromArchived();
    }
}
