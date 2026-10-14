namespace Acme.Workforce.Compliance.Tests.Audit.Bundle;

using Acme.Workforce.Compliance.Audit.Bundle;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class WeightedAuditMetricEngineChecks
{
    public static void WeightedAuditMetricEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAuditMetricEngine.DefaultWeightedAuditMetricEngine(Flags.NewRegistry());
        var item = WeightedAuditMetric.NewWeightedAuditMetric("code-336", "title-696");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAuditMetricEngineKeepsResultsWithinBounds();
    }
}
