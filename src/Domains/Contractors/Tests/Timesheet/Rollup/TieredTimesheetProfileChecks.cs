namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TieredTimesheetProfileChecks
{
    public static void TieredTimesheetProfileStartsInRequested()
    {
        var item = TieredTimesheetProfile.NewTieredTimesheetProfile("code-298", "title-477");
        Check.Equal(item.Status, TieredTimesheetProfileStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredTimesheetProfileMovesToGranted()
    {
        var item = TieredTimesheetProfile.NewTieredTimesheetProfile("code-298", "title-477");
        try
        {
            item.MoveTo(TieredTimesheetProfileStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredTimesheetProfileStatus.Granted, "status");
    }

    public static void TieredTimesheetProfileRejectsMovesFromExpired()
    {
        var item = TieredTimesheetProfile.NewTieredTimesheetProfile("code-298", "title-477");
        item.Status = TieredTimesheetProfileStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredTimesheetProfileStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredTimesheetProfileStartsInRequested();
        TieredTimesheetProfileMovesToGranted();
        TieredTimesheetProfileRejectsMovesFromExpired();
    }
}
