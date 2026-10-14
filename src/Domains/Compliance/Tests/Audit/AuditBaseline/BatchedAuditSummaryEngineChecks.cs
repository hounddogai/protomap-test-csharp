namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class BatchedAuditSummaryEngineChecks
{
    public static void BatchedAuditSummaryEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedAuditSummaryEngine.DefaultBatchedAuditSummaryEngine();
        var item = BatchedAuditSummary.NewBatchedAuditSummary("code-790", "label-893");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedAuditSummaryEngineKeepsResultsWithinBounds();
    }
}
