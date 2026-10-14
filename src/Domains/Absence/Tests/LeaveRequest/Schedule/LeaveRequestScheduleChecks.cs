namespace Acme.Workforce.Absence.Tests.LeaveRequest.Schedule;

using Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LeaveRequestScheduleChecks
{
    public static void LeaveRequestScheduleStartsInIdle()
    {
        var item = LeaveRequestSchedule.NewLeaveRequestSchedule("code-293", "title-713");
        Check.Equal(item.Status, LeaveRequestScheduleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestScheduleMovesToWarming()
    {
        var item = LeaveRequestSchedule.NewLeaveRequestSchedule("code-293", "title-713");
        try
        {
            item.MoveTo(LeaveRequestScheduleStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestScheduleStatus.Warming, "status");
    }

    public static void LeaveRequestScheduleRejectsMovesFromStopped()
    {
        var item = LeaveRequestSchedule.NewLeaveRequestSchedule("code-293", "title-713");
        item.Status = LeaveRequestScheduleStatus.Stopped;
        Check.Fails(() => item.MoveTo(LeaveRequestScheduleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LeaveRequestScheduleStartsInIdle();
        LeaveRequestScheduleMovesToWarming();
        LeaveRequestScheduleRejectsMovesFromStopped();
    }
}
