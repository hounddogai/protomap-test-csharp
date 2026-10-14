namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LeaveRequestAssignmentChecks
{
    public static void LeaveRequestAssignmentStartsInPlanned()
    {
        var item = LeaveRequestAssignment.NewLeaveRequestAssignment("code-142", "title-740");
        Check.Equal(item.Status, LeaveRequestAssignmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestAssignmentMovesToScheduled()
    {
        var item = LeaveRequestAssignment.NewLeaveRequestAssignment("code-142", "title-740");
        try
        {
            item.MoveTo(LeaveRequestAssignmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestAssignmentStatus.Scheduled, "status");
    }

    public static void LeaveRequestAssignmentRejectsMovesFromCancelled()
    {
        var item = LeaveRequestAssignment.NewLeaveRequestAssignment("code-142", "title-740");
        item.Status = LeaveRequestAssignmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LeaveRequestAssignmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LeaveRequestAssignmentStartsInPlanned();
        LeaveRequestAssignmentMovesToScheduled();
        LeaveRequestAssignmentRejectsMovesFromCancelled();
    }
}
