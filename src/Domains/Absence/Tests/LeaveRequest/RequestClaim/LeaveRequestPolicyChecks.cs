namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class LeaveRequestPolicyChecks
{
    public static void LeaveRequestPolicyStartsInNew()
    {
        var item = LeaveRequestPolicy.NewLeaveRequestPolicy("code-292", "label-859");
        Check.Equal(item.Status, LeaveRequestPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestPolicyMovesToRunning()
    {
        var item = LeaveRequestPolicy.NewLeaveRequestPolicy("code-292", "label-859");
        try
        {
            item.MoveTo(LeaveRequestPolicyStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestPolicyStatus.Running, "status");
    }

    public static void LeaveRequestPolicyRejectsMovesFromFailed()
    {
        var item = LeaveRequestPolicy.NewLeaveRequestPolicy("code-292", "label-859");
        item.Status = LeaveRequestPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(LeaveRequestPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        LeaveRequestPolicyStartsInNew();
        LeaveRequestPolicyMovesToRunning();
        LeaveRequestPolicyRejectsMovesFromFailed();
    }
}
