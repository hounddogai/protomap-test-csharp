namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestMappingChecks
{
    public static void ScheduledLeaveRequestMappingStartsInOpen()
    {
        var item = ScheduledLeaveRequestMapping.NewScheduledLeaveRequestMapping("code-34", "title-965");
        Check.Equal(item.Status, ScheduledLeaveRequestMappingStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledLeaveRequestMappingMovesToReopened()
    {
        var item = ScheduledLeaveRequestMapping.NewScheduledLeaveRequestMapping("code-34", "title-965");
        try
        {
            item.MoveTo(ScheduledLeaveRequestMappingStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledLeaveRequestMappingStatus.Reopened, "status");
    }

    public static void ScheduledLeaveRequestMappingRejectsMovesFromArchived()
    {
        var item = ScheduledLeaveRequestMapping.NewScheduledLeaveRequestMapping("code-34", "title-965");
        item.Status = ScheduledLeaveRequestMappingStatus.Archived;
        Check.Fails(() => item.MoveTo(ScheduledLeaveRequestMappingStatus.Open), "move");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestMappingStartsInOpen();
        ScheduledLeaveRequestMappingMovesToReopened();
        ScheduledLeaveRequestMappingRejectsMovesFromArchived();
    }
}
