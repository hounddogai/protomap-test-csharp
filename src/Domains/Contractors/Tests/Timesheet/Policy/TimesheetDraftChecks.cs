namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetDraftChecks
{
    public static void TimesheetDraftStartsInIdle()
    {
        var item = TimesheetDraft.NewTimesheetDraft("code-799", "label-733");
        Check.Equal(item.Status, TimesheetDraftStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetDraftMovesToReady()
    {
        var item = TimesheetDraft.NewTimesheetDraft("code-799", "label-733");
        try
        {
            item.MoveTo(TimesheetDraftStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetDraftStatus.Ready, "status");
    }

    public static void TimesheetDraftRejectsMovesFromStopped()
    {
        var item = TimesheetDraft.NewTimesheetDraft("code-799", "label-733");
        item.Status = TimesheetDraftStatus.Stopped;
        Check.Fails(() => item.MoveTo(TimesheetDraftStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TimesheetDraftStartsInIdle();
        TimesheetDraftMovesToReady();
        TimesheetDraftRejectsMovesFromStopped();
    }
}
