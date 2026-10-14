namespace Acme.Workforce.Compensation.Tests.EquityGrant.Group;

using Acme.Workforce.Compensation.EquityGrant.Group;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class EquityGrantQuotaEngineChecks
{
    public static void EquityGrantQuotaEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantQuotaEngine.DefaultEquityGrantQuotaEngine(Flags.NewRegistry());
        var item = EquityGrantQuota.NewEquityGrantQuota("code-672", "name-911");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantQuotaEngineKeepsResultsWithinBounds();
    }
}
