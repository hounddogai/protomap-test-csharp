namespace Acme.Workforce.Workplace.Tests.AccessBadge.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class BatchedAccessBadgeTargetEngineChecks
{
    public static void BatchedAccessBadgeTargetEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedAccessBadgeTargetEngine.DefaultBatchedAccessBadgeTargetEngine(Flags.NewRegistry());
        var item = BatchedAccessBadgeTarget.NewBatchedAccessBadgeTarget("code-10", "title-996");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedAccessBadgeTargetEngineKeepsResultsWithinBounds();
    }
}
