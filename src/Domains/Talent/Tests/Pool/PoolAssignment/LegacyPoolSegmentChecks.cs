namespace Acme.Workforce.Talent.Tests.Pool.PoolAssignment;

using Acme.Workforce.Talent.Pool.PoolAssignment;

/// <summary>Checks of pool assignment code, run by the project's runner.</summary>
public static class LegacyPoolSegmentChecks
{
    public static void LegacyPoolSegmentStartsInPlanned()
    {
        var item = LegacyPoolSegment.NewLegacyPoolSegment("code-776", "name-420");
        Check.Equal(item.Status, LegacyPoolSegmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyPoolSegmentMovesToCompleted()
    {
        var item = LegacyPoolSegment.NewLegacyPoolSegment("code-776", "name-420");
        try
        {
            item.MoveTo(LegacyPoolSegmentStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyPoolSegmentStatus.Completed, "status");
    }

    public static void LegacyPoolSegmentRejectsMovesFromCancelled()
    {
        var item = LegacyPoolSegment.NewLegacyPoolSegment("code-776", "name-420");
        item.Status = LegacyPoolSegmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LegacyPoolSegmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LegacyPoolSegmentStartsInPlanned();
        LegacyPoolSegmentMovesToCompleted();
        LegacyPoolSegmentRejectsMovesFromCancelled();
    }
}
