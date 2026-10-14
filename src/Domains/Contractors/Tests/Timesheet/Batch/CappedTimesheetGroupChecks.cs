namespace Acme.Workforce.Contractors.Tests.Timesheet.Batch;

using Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class CappedTimesheetGroupChecks
{
    public static void CappedTimesheetGroupStartsInCreated()
    {
        var item = CappedTimesheetGroup.NewCappedTimesheetGroup("code-900", "name-137");
        Check.Equal(item.Status, CappedTimesheetGroupStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedTimesheetGroupMovesToPublished()
    {
        var item = CappedTimesheetGroup.NewCappedTimesheetGroup("code-900", "name-137");
        try
        {
            item.MoveTo(CappedTimesheetGroupStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedTimesheetGroupStatus.Published, "status");
    }

    public static void CappedTimesheetGroupRejectsMovesFromRetired()
    {
        var item = CappedTimesheetGroup.NewCappedTimesheetGroup("code-900", "name-137");
        item.Status = CappedTimesheetGroupStatus.Retired;
        Check.Fails(() => item.MoveTo(CappedTimesheetGroupStatus.Created), "move");
    }

    public static void RunAll()
    {
        CappedTimesheetGroupStartsInCreated();
        CappedTimesheetGroupMovesToPublished();
        CappedTimesheetGroupRejectsMovesFromRetired();
    }
}
