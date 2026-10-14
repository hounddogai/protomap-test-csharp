namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedAuditRecordEngineChecks
{
    public static void WeightedAuditRecordEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedAuditRecordEngine.DefaultWeightedAuditRecordEngine();
        var item = WeightedAuditRecord.NewWeightedAuditRecord("code-249", "label-597");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedAuditRecordEngineKeepsResultsWithinBounds();
    }
}
