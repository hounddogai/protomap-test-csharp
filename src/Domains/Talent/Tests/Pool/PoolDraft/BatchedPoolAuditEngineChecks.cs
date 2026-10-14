namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class BatchedPoolAuditEngineChecks
{
    public static void BatchedPoolAuditEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedPoolAuditEngine.DefaultBatchedPoolAuditEngine();
        var item = BatchedPoolAudit.NewBatchedPoolAudit("code-782", "name-650");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedPoolAuditEngineKeepsResultsWithinBounds();
    }
}
