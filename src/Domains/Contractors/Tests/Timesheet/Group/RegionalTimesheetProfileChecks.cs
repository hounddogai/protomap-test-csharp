namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class RegionalTimesheetProfileChecks
{
    public static void RegionalTimesheetProfileStartsInDraft()
    {
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("code-824", "name-595");
        Check.Equal(item.Status, RegionalTimesheetProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalTimesheetProfileMovesToPending()
    {
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("code-824", "name-595");
        try
        {
            item.MoveTo(RegionalTimesheetProfileStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalTimesheetProfileStatus.Pending, "status");
    }

    public static void RegionalTimesheetProfileRejectsMovesFromClosed()
    {
        var item = RegionalTimesheetProfile.NewRegionalTimesheetProfile("code-824", "name-595");
        item.Status = RegionalTimesheetProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalTimesheetProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalTimesheetProfileStartsInDraft();
        RegionalTimesheetProfileMovesToPending();
        RegionalTimesheetProfileRejectsMovesFromClosed();
    }
}
