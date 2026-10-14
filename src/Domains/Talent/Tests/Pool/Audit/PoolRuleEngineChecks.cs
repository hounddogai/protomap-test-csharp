namespace Acme.Workforce.Talent.Tests.Pool.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.Pool.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PoolRuleEngineChecks
{
    public static void PoolRuleEngineKeepsResultsWithinBounds()
    {
        var engine = PoolRuleEngine.DefaultPoolRuleEngine(Flags.NewRegistry());
        var item = PoolRule.NewPoolRule("code-950", "label-949");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PoolRuleEngineKeepsResultsWithinBounds();
    }
}
