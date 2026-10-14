namespace Acme.Workforce.Contractors.Tests.Timesheet.Review;

using Acme.Workforce.Contractors.Timesheet.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class CappedTimesheetThresholdChecks
{
    public static void CappedTimesheetThresholdStartsInRequested()
    {
        var item = CappedTimesheetThreshold.NewCappedTimesheetThreshold("code-750", "name-420");
        Check.Equal(item.Status, CappedTimesheetThresholdStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedTimesheetThresholdMovesToExpired()
    {
        var item = CappedTimesheetThreshold.NewCappedTimesheetThreshold("code-750", "name-420");
        try
        {
            item.MoveTo(CappedTimesheetThresholdStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedTimesheetThresholdStatus.Expired, "status");
    }

    public static void CappedTimesheetThresholdRejectsMovesFromExpired()
    {
        var item = CappedTimesheetThreshold.NewCappedTimesheetThreshold("code-750", "name-420");
        item.Status = CappedTimesheetThresholdStatus.Expired;
        Check.Fails(() => item.MoveTo(CappedTimesheetThresholdStatus.Requested), "move");
    }

    public static void RunAll()
    {
        CappedTimesheetThresholdStartsInRequested();
        CappedTimesheetThresholdMovesToExpired();
        CappedTimesheetThresholdRejectsMovesFromExpired();
    }
}
