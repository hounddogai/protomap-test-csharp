namespace Acme.Workforce.Talent.Tests.Pool.Threshold;

using Acme.Workforce.Talent.Pool.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PoolDigestChecks
{
    public static void PoolDigestStartsInIdle()
    {
        var item = PoolDigest.NewPoolDigest("code-920", "title-444");
        Check.Equal(item.Status, PoolDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolDigestMovesToDraining()
    {
        var item = PoolDigest.NewPoolDigest("code-920", "title-444");
        try
        {
            item.MoveTo(PoolDigestStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolDigestStatus.Draining, "status");
    }

    public static void PoolDigestRejectsMovesFromStopped()
    {
        var item = PoolDigest.NewPoolDigest("code-920", "title-444");
        item.Status = PoolDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(PoolDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PoolDigestStartsInIdle();
        PoolDigestMovesToDraining();
        PoolDigestRejectsMovesFromStopped();
    }
}
