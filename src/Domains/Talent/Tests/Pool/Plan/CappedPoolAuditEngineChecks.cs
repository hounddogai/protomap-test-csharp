namespace Acme.Workforce.Talent.Tests.Pool.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class CappedPoolAuditEngineChecks
{
    public static void CappedPoolAuditEngineKeepsResultsWithinBounds()
    {
        var engine = CappedPoolAuditEngine.DefaultCappedPoolAuditEngine(Flags.NewRegistry());
        var item = CappedPoolAudit.NewCappedPoolAudit("code-575", "label-672");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedPoolAuditEngineKeepsResultsWithinBounds();
    }
}
