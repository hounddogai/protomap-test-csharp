namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestBaselineChecks
{
    public static void ScheduledLeaveRequestBaselineStartsInPlanned()
    {
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("code-100", "title-97");
        Check.Equal(item.Status, ScheduledLeaveRequestBaselineStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledLeaveRequestBaselineMovesToInProgress()
    {
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("code-100", "title-97");
        try
        {
            item.MoveTo(ScheduledLeaveRequestBaselineStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledLeaveRequestBaselineStatus.InProgress, "status");
    }

    public static void ScheduledLeaveRequestBaselineRejectsMovesFromCancelled()
    {
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("code-100", "title-97");
        item.Status = ScheduledLeaveRequestBaselineStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ScheduledLeaveRequestBaselineStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestBaselineStartsInPlanned();
        ScheduledLeaveRequestBaselineMovesToInProgress();
        ScheduledLeaveRequestBaselineRejectsMovesFromCancelled();
    }
}
