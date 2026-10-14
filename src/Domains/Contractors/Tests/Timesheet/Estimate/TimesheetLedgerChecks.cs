namespace Acme.Workforce.Contractors.Tests.Timesheet.Estimate;

using Acme.Workforce.Contractors.Timesheet.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class TimesheetLedgerChecks
{
    public static void TimesheetLedgerStartsInCreated()
    {
        var item = TimesheetLedger.NewTimesheetLedger("code-949", "label-107");
        Check.Equal(item.Status, TimesheetLedgerStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetLedgerMovesToRetired()
    {
        var item = TimesheetLedger.NewTimesheetLedger("code-949", "label-107");
        try
        {
            item.MoveTo(TimesheetLedgerStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetLedgerStatus.Retired, "status");
    }

    public static void TimesheetLedgerRejectsMovesFromRetired()
    {
        var item = TimesheetLedger.NewTimesheetLedger("code-949", "label-107");
        item.Status = TimesheetLedgerStatus.Retired;
        Check.Fails(() => item.MoveTo(TimesheetLedgerStatus.Created), "move");
    }

    public static void RunAll()
    {
        TimesheetLedgerStartsInCreated();
        TimesheetLedgerMovesToRetired();
        TimesheetLedgerRejectsMovesFromRetired();
    }
}
