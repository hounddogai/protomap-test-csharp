namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

/// <summary>Checks of badge audit code, run by the project's runner.</summary>
public static class WeightedAccessBadgeBatchEngineChecks
{
    public static void WeightedAccessBadgeBatchEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAccessBadgeBatchEngine.DefaultWeightedAccessBadgeBatchEngine();
        var item = WeightedAccessBadgeBatch.NewWeightedAccessBadgeBatch("code-430", "label-124");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAccessBadgeBatchEngineKeepsResultsWithinBounds();
    }
}
