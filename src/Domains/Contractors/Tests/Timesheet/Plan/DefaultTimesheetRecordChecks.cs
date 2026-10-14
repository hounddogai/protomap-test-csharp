namespace Acme.Workforce.Contractors.Tests.Timesheet.Plan;

using Acme.Workforce.Contractors.Timesheet.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultTimesheetRecordChecks
{
    public static void DefaultTimesheetRecordStartsInOpen()
    {
        var item = DefaultTimesheetRecord.NewDefaultTimesheetRecord("code-250", "name-74");
        Check.Equal(item.Status, DefaultTimesheetRecordStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultTimesheetRecordMovesToInReview()
    {
        var item = DefaultTimesheetRecord.NewDefaultTimesheetRecord("code-250", "name-74");
        try
        {
            item.MoveTo(DefaultTimesheetRecordStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultTimesheetRecordStatus.InReview, "status");
    }

    public static void DefaultTimesheetRecordRejectsMovesFromArchived()
    {
        var item = DefaultTimesheetRecord.NewDefaultTimesheetRecord("code-250", "name-74");
        item.Status = DefaultTimesheetRecordStatus.Archived;
        Check.Fails(() => item.MoveTo(DefaultTimesheetRecordStatus.Open), "move");
    }

    public static void RunAll()
    {
        DefaultTimesheetRecordStartsInOpen();
        DefaultTimesheetRecordMovesToInReview();
        DefaultTimesheetRecordRejectsMovesFromArchived();
    }
}
