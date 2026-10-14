namespace Acme.Workforce.Absence.Tests.LeaveRequest.Assignment;

using Acme.Workforce.Absence.LeaveRequest.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class LeaveRequestBundleChecks
{
    public static void LeaveRequestBundleStartsInIdle()
    {
        var item = LeaveRequestBundle.NewLeaveRequestBundle("code-774", "name-655");
        Check.Equal(item.Status, LeaveRequestBundleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestBundleMovesToReady()
    {
        var item = LeaveRequestBundle.NewLeaveRequestBundle("code-774", "name-655");
        try
        {
            item.MoveTo(LeaveRequestBundleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestBundleStatus.Ready, "status");
    }

    public static void LeaveRequestBundleRejectsMovesFromStopped()
    {
        var item = LeaveRequestBundle.NewLeaveRequestBundle("code-774", "name-655");
        item.Status = LeaveRequestBundleStatus.Stopped;
        Check.Fails(() => item.MoveTo(LeaveRequestBundleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LeaveRequestBundleStartsInIdle();
        LeaveRequestBundleMovesToReady();
        LeaveRequestBundleRejectsMovesFromStopped();
    }
}
