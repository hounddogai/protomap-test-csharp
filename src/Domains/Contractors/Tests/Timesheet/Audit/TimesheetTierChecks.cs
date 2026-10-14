namespace Acme.Workforce.Contractors.Tests.Timesheet.Audit;

using Acme.Workforce.Contractors.Timesheet.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class TimesheetTierChecks
{
    public static void TimesheetTierStartsInIdle()
    {
        var item = TimesheetTier.NewTimesheetTier("code-368", "title-933");
        Check.Equal(item.Status, TimesheetTierStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetTierMovesToDraining()
    {
        var item = TimesheetTier.NewTimesheetTier("code-368", "title-933");
        try
        {
            item.MoveTo(TimesheetTierStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetTierStatus.Draining, "status");
    }

    public static void TimesheetTierRejectsMovesFromStopped()
    {
        var item = TimesheetTier.NewTimesheetTier("code-368", "title-933");
        item.Status = TimesheetTierStatus.Stopped;
        Check.Fails(() => item.MoveTo(TimesheetTierStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TimesheetTierStartsInIdle();
        TimesheetTierMovesToDraining();
        TimesheetTierRejectsMovesFromStopped();
    }
}
