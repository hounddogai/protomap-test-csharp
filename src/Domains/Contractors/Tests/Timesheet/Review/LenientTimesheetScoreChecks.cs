namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientTimesheetScoreChecks
{
    public static void LenientTimesheetScoreStartsInIdle()
    {
        var item = LenientTimesheetScore.NewLenientTimesheetScore("code-811", "label-457");
        Check.Equal(item.Status, LenientTimesheetScoreStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientTimesheetScoreMovesToStopped()
    {
        var item = LenientTimesheetScore.NewLenientTimesheetScore("code-811", "label-457");
        try
        {
            item.MoveTo(LenientTimesheetScoreStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientTimesheetScoreStatus.Stopped, "status");
    }

    public static void LenientTimesheetScoreRejectsMovesFromStopped()
    {
        var item = LenientTimesheetScore.NewLenientTimesheetScore("code-811", "label-457");
        item.Status = LenientTimesheetScoreStatus.Stopped;
        Check.Fails(() => item.MoveTo(LenientTimesheetScoreStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LenientTimesheetScoreStartsInIdle();
        LenientTimesheetScoreMovesToStopped();
        LenientTimesheetScoreRejectsMovesFromStopped();
    }
}
