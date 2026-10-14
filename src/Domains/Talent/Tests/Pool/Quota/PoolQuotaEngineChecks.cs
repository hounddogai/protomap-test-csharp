namespace Acme.Workforce.Talent.Tests.Pool.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class PoolQuotaEngineChecks
{
    public static void PoolQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = PoolQuotaEngine.DefaultPoolQuotaEngine(Flags.NewRegistry());
        var item = PoolQuota.NewPoolQuota("code-125", "label-436");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolQuotaEngineKeepsResultsWithinBounds();
    }
}
