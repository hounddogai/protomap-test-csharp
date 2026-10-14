namespace Acme.Workforce.Payrollops.Tests.PayRun.Adjustment;

using Acme.Workforce.Payrollops.PayRun.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class RollingPayRunDigestChecks
{
    public static void RollingPayRunDigestStartsInPlanned()
    {
        var item = RollingPayRunDigest.NewRollingPayRunDigest("code-606", "title-78");
        Check.Equal(item.Status, RollingPayRunDigestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingPayRunDigestMovesToScheduled()
    {
        var item = RollingPayRunDigest.NewRollingPayRunDigest("code-606", "title-78");
        try
        {
            item.MoveTo(RollingPayRunDigestStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingPayRunDigestStatus.Scheduled, "status");
    }

    public static void RollingPayRunDigestRejectsMovesFromCancelled()
    {
        var item = RollingPayRunDigest.NewRollingPayRunDigest("code-606", "title-78");
        item.Status = RollingPayRunDigestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingPayRunDigestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingPayRunDigestStartsInPlanned();
        RollingPayRunDigestMovesToScheduled();
        RollingPayRunDigestRejectsMovesFromCancelled();
    }
}
