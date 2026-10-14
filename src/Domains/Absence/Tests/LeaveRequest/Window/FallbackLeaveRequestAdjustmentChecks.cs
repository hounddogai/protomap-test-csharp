namespace Acme.Workforce.Absence.Tests.LeaveRequest.Window;

using Acme.Workforce.Absence.LeaveRequest.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class FallbackLeaveRequestAdjustmentChecks
{
    public static void FallbackLeaveRequestAdjustmentStartsInIdle()
    {
        var item = FallbackLeaveRequestAdjustment.NewFallbackLeaveRequestAdjustment("code-472", "label-243");
        Check.Equal(item.Status, FallbackLeaveRequestAdjustmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackLeaveRequestAdjustmentMovesToWarming()
    {
        var item = FallbackLeaveRequestAdjustment.NewFallbackLeaveRequestAdjustment("code-472", "label-243");
        try
        {
            item.MoveTo(FallbackLeaveRequestAdjustmentStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackLeaveRequestAdjustmentStatus.Warming, "status");
    }

    public static void FallbackLeaveRequestAdjustmentRejectsMovesFromStopped()
    {
        var item = FallbackLeaveRequestAdjustment.NewFallbackLeaveRequestAdjustment("code-472", "label-243");
        item.Status = FallbackLeaveRequestAdjustmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(FallbackLeaveRequestAdjustmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        FallbackLeaveRequestAdjustmentStartsInIdle();
        FallbackLeaveRequestAdjustmentMovesToWarming();
        FallbackLeaveRequestAdjustmentRejectsMovesFromStopped();
    }
}
