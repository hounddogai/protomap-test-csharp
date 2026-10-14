namespace Acme.Workforce.Contractors.Tests.Timesheet.RecordItem;

using Acme.Workforce.Contractors.Timesheet.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class TimesheetRecordChecks
{
    public static void TimesheetRecordStartsInDraft()
    {
        var item = TimesheetRecord.NewTimesheetRecord("code-198", "name-432");
        Check.Equal(item.Status, TimesheetRecordStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetRecordMovesToActive()
    {
        var item = TimesheetRecord.NewTimesheetRecord("code-198", "name-432");
        try
        {
            item.MoveTo(TimesheetRecordStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetRecordStatus.Active, "status");
    }

    public static void TimesheetRecordRejectsMovesFromClosed()
    {
        var item = TimesheetRecord.NewTimesheetRecord("code-198", "name-432");
        item.Status = TimesheetRecordStatus.Closed;
        Check.Fails(() => item.MoveTo(TimesheetRecordStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TimesheetRecordStartsInDraft();
        TimesheetRecordMovesToActive();
        TimesheetRecordRejectsMovesFromClosed();
    }
}
