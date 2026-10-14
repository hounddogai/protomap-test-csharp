namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestMilestoneChecks
{
    public static void PrimaryLeaveRequestMilestoneStartsInIdle()
    {
        var item = PrimaryLeaveRequestMilestone.NewPrimaryLeaveRequestMilestone("code-82", "title-385");
        Check.Equal(item.Status, PrimaryLeaveRequestMilestoneStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryLeaveRequestMilestoneMovesToDraining()
    {
        var item = PrimaryLeaveRequestMilestone.NewPrimaryLeaveRequestMilestone("code-82", "title-385");
        try
        {
            item.MoveTo(PrimaryLeaveRequestMilestoneStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryLeaveRequestMilestoneStatus.Draining, "status");
    }

    public static void PrimaryLeaveRequestMilestoneRejectsMovesFromStopped()
    {
        var item = PrimaryLeaveRequestMilestone.NewPrimaryLeaveRequestMilestone("code-82", "title-385");
        item.Status = PrimaryLeaveRequestMilestoneStatus.Stopped;
        Check.Fails(() => item.MoveTo(PrimaryLeaveRequestMilestoneStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PrimaryLeaveRequestMilestoneStartsInIdle();
        PrimaryLeaveRequestMilestoneMovesToDraining();
        PrimaryLeaveRequestMilestoneRejectsMovesFromStopped();
    }
}
