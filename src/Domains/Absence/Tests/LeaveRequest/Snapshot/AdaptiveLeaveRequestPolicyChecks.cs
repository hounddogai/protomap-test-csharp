namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class AdaptiveLeaveRequestPolicyChecks
{
    public static void AdaptiveLeaveRequestPolicyStartsInIdle()
    {
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("code-562", "name-723");
        Check.Equal(item.Status, AdaptiveLeaveRequestPolicyStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveLeaveRequestPolicyMovesToReady()
    {
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("code-562", "name-723");
        try
        {
            item.MoveTo(AdaptiveLeaveRequestPolicyStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveLeaveRequestPolicyStatus.Ready, "status");
    }

    public static void AdaptiveLeaveRequestPolicyRejectsMovesFromStopped()
    {
        var item = AdaptiveLeaveRequestPolicy.NewAdaptiveLeaveRequestPolicy("code-562", "name-723");
        item.Status = AdaptiveLeaveRequestPolicyStatus.Stopped;
        Check.Fails(() => item.MoveTo(AdaptiveLeaveRequestPolicyStatus.Idle), "move");
    }

    public static void RunAll()
    {
        AdaptiveLeaveRequestPolicyStartsInIdle();
        AdaptiveLeaveRequestPolicyMovesToReady();
        AdaptiveLeaveRequestPolicyRejectsMovesFromStopped();
    }
}
