namespace Acme.Workforce.Absence.Tests.LeaveRequest.Adjustment;

using Acme.Workforce.Absence.LeaveRequest.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LeaveRequestMappingChecks
{
    public static void LeaveRequestMappingStartsInRequested()
    {
        var item = LeaveRequestMapping.NewLeaveRequestMapping("code-798", "title-767");
        Check.Equal(item.Status, LeaveRequestMappingStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestMappingMovesToRevoked()
    {
        var item = LeaveRequestMapping.NewLeaveRequestMapping("code-798", "title-767");
        try
        {
            item.MoveTo(LeaveRequestMappingStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestMappingStatus.Revoked, "status");
    }

    public static void LeaveRequestMappingRejectsMovesFromExpired()
    {
        var item = LeaveRequestMapping.NewLeaveRequestMapping("code-798", "title-767");
        item.Status = LeaveRequestMappingStatus.Expired;
        Check.Fails(() => item.MoveTo(LeaveRequestMappingStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LeaveRequestMappingStartsInRequested();
        LeaveRequestMappingMovesToRevoked();
        LeaveRequestMappingRejectsMovesFromExpired();
    }
}
