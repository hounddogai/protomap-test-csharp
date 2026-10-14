namespace Acme.Workforce.Workplace.Tests.AccessBadge.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccessBadgeSegmentEngineChecks
{
    public static void AccessBadgeSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeSegmentEngine.DefaultAccessBadgeSegmentEngine(Flags.NewRegistry());
        var item = AccessBadgeSegment.NewAccessBadgeSegment("code-891", "name-485");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeSegmentEngineKeepsResultsWithinBounds();
    }
}
