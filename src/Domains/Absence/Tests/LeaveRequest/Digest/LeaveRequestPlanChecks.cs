namespace Acme.Workforce.Absence.Tests.LeaveRequest.Digest;

using Acme.Workforce.Absence.LeaveRequest.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LeaveRequestPlanChecks
{
    public static void LeaveRequestPlanStartsInRequested()
    {
        var item = LeaveRequestPlan.NewLeaveRequestPlan("code-611", "name-401");
        Check.Equal(item.Status, LeaveRequestPlanStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestPlanMovesToRevoked()
    {
        var item = LeaveRequestPlan.NewLeaveRequestPlan("code-611", "name-401");
        try
        {
            item.MoveTo(LeaveRequestPlanStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestPlanStatus.Revoked, "status");
    }

    public static void LeaveRequestPlanRejectsMovesFromExpired()
    {
        var item = LeaveRequestPlan.NewLeaveRequestPlan("code-611", "name-401");
        item.Status = LeaveRequestPlanStatus.Expired;
        Check.Fails(() => item.MoveTo(LeaveRequestPlanStatus.Requested), "move");
    }

    public static void RunAll()
    {
        LeaveRequestPlanStartsInRequested();
        LeaveRequestPlanMovesToRevoked();
        LeaveRequestPlanRejectsMovesFromExpired();
    }
}
