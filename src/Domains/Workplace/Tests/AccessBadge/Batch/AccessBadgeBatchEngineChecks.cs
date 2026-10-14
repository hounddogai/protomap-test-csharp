namespace Acme.Workforce.Workplace.Tests.AccessBadge.Batch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class AccessBadgeBatchEngineChecks
{
    public static void AccessBadgeBatchEngineKeepsResultsWithinBounds()
    {
        var engine = AccessBadgeBatchEngine.DefaultAccessBadgeBatchEngine(Flags.NewRegistry());
        var item = AccessBadgeBatch.NewAccessBadgeBatch("code-739", "name-687");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AccessBadgeBatchEngineKeepsResultsWithinBounds();
    }
}
