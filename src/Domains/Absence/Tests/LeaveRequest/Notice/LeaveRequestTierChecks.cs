namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestTierChecks
{
    public static void LeaveRequestTierStartsInNew()
    {
        var item = LeaveRequestTier.NewLeaveRequestTier("code-991", "label-294");
        Check.Equal(item.Status, LeaveRequestTierStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestTierMovesToRunning()
    {
        var item = LeaveRequestTier.NewLeaveRequestTier("code-991", "label-294");
        try
        {
            item.MoveTo(LeaveRequestTierStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestTierStatus.Running, "status");
    }

    public static void LeaveRequestTierRejectsMovesFromFailed()
    {
        var item = LeaveRequestTier.NewLeaveRequestTier("code-991", "label-294");
        item.Status = LeaveRequestTierStatus.Failed;
        Check.Fails(() => item.MoveTo(LeaveRequestTierStatus.New), "move");
    }

    public static void RunAll()
    {
        LeaveRequestTierStartsInNew();
        LeaveRequestTierMovesToRunning();
        LeaveRequestTierRejectsMovesFromFailed();
    }
}
