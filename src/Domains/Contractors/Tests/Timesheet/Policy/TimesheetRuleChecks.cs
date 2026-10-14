namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetRuleChecks
{
    public static void TimesheetRuleStartsInDraft()
    {
        var item = TimesheetRule.NewTimesheetRule("code-650", "title-661");
        Check.Equal(item.Status, TimesheetRuleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetRuleMovesToPending()
    {
        var item = TimesheetRule.NewTimesheetRule("code-650", "title-661");
        try
        {
            item.MoveTo(TimesheetRuleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetRuleStatus.Pending, "status");
    }

    public static void TimesheetRuleRejectsMovesFromClosed()
    {
        var item = TimesheetRule.NewTimesheetRule("code-650", "title-661");
        item.Status = TimesheetRuleStatus.Closed;
        Check.Fails(() => item.MoveTo(TimesheetRuleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TimesheetRuleStartsInDraft();
        TimesheetRuleMovesToPending();
        TimesheetRuleRejectsMovesFromClosed();
    }
}
