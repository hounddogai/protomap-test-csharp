namespace Acme.Workforce.Contractors.Tests.Timesheet.Reservation;

using Acme.Workforce.Contractors.Timesheet.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TimesheetScoreChecks
{
    public static void TimesheetScoreStartsInDraft()
    {
        var item = TimesheetScore.NewTimesheetScore("code-286", "title-173");
        Check.Equal(item.Status, TimesheetScoreStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetScoreMovesToPending()
    {
        var item = TimesheetScore.NewTimesheetScore("code-286", "title-173");
        try
        {
            item.MoveTo(TimesheetScoreStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetScoreStatus.Pending, "status");
    }

    public static void TimesheetScoreRejectsMovesFromClosed()
    {
        var item = TimesheetScore.NewTimesheetScore("code-286", "title-173");
        item.Status = TimesheetScoreStatus.Closed;
        Check.Fails(() => item.MoveTo(TimesheetScoreStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TimesheetScoreStartsInDraft();
        TimesheetScoreMovesToPending();
        TimesheetScoreRejectsMovesFromClosed();
    }
}
