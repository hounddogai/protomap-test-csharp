namespace Acme.Workforce.Talent.Tests.Pool.Rule;

using Acme.Workforce.Talent.Pool.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PoolSegmentChecks
{
    public static void PoolSegmentStartsInIdle()
    {
        var item = PoolSegment.NewPoolSegment("code-338", "title-125");
        Check.Equal(item.Status, PoolSegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolSegmentMovesToDraining()
    {
        var item = PoolSegment.NewPoolSegment("code-338", "title-125");
        try
        {
            item.MoveTo(PoolSegmentStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolSegmentStatus.Draining, "status");
    }

    public static void PoolSegmentRejectsMovesFromStopped()
    {
        var item = PoolSegment.NewPoolSegment("code-338", "title-125");
        item.Status = PoolSegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(PoolSegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PoolSegmentStartsInIdle();
        PoolSegmentMovesToDraining();
        PoolSegmentRejectsMovesFromStopped();
    }
}
