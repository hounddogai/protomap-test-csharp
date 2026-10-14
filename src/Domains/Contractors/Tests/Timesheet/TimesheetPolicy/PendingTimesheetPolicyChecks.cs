namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetPolicy;

using Acme.Workforce.Contractors.Timesheet.TimesheetPolicy;

/// <summary>Checks of timesheet policy code, run by the project's runner.</summary>
public static class PendingTimesheetPolicyChecks
{
    public static void PendingTimesheetPolicyStartsInOpen()
    {
        var item = PendingTimesheetPolicy.NewPendingTimesheetPolicy("code-350", "title-741");
        Check.Equal(item.Status, PendingTimesheetPolicyStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingTimesheetPolicyMovesToInReview()
    {
        var item = PendingTimesheetPolicy.NewPendingTimesheetPolicy("code-350", "title-741");
        try
        {
            item.MoveTo(PendingTimesheetPolicyStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingTimesheetPolicyStatus.InReview, "status");
    }

    public static void PendingTimesheetPolicyRejectsMovesFromArchived()
    {
        var item = PendingTimesheetPolicy.NewPendingTimesheetPolicy("code-350", "title-741");
        item.Status = PendingTimesheetPolicyStatus.Archived;
        Check.Fails(() => item.MoveTo(PendingTimesheetPolicyStatus.Open), "move");
    }

    public static void RunAll()
    {
        PendingTimesheetPolicyStartsInOpen();
        PendingTimesheetPolicyMovesToInReview();
        PendingTimesheetPolicyRejectsMovesFromArchived();
    }
}
