namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentAllocation;

using Acme.Workforce.Compensation.Adjustment.AdjustmentAllocation;

/// <summary>Checks of adjustment allocation code, run by the project's runner.</summary>
public static class RollingAdjustmentNoticeChecks
{
    public static void RollingAdjustmentNoticeStartsInIdle()
    {
        var item = RollingAdjustmentNotice.NewRollingAdjustmentNotice("code-780", "title-672");
        Check.Equal(item.Status, RollingAdjustmentNoticeStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAdjustmentNoticeMovesToReady()
    {
        var item = RollingAdjustmentNotice.NewRollingAdjustmentNotice("code-780", "title-672");
        try
        {
            item.MoveTo(RollingAdjustmentNoticeStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAdjustmentNoticeStatus.Ready, "status");
    }

    public static void RollingAdjustmentNoticeRejectsMovesFromStopped()
    {
        var item = RollingAdjustmentNotice.NewRollingAdjustmentNotice("code-780", "title-672");
        item.Status = RollingAdjustmentNoticeStatus.Stopped;
        Check.Fails(() => item.MoveTo(RollingAdjustmentNoticeStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RollingAdjustmentNoticeStartsInIdle();
        RollingAdjustmentNoticeMovesToReady();
        RollingAdjustmentNoticeRejectsMovesFromStopped();
    }
}
