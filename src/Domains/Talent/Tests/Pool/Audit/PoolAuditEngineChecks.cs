namespace Acme.Workforce.Talent.Tests.Pool.Audit;

using Acme.Workforce.Talent.Pool.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PoolAuditEngineChecks
{
    public static void PoolAuditEngineKeepsResultsWithinBounds()
    {
        var engine = PoolAuditEngine.DefaultPoolAuditEngine();
        var item = PoolAudit.NewPoolAudit("code-510", "title-142");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolAuditEngineKeepsResultsWithinBounds();
    }
}
