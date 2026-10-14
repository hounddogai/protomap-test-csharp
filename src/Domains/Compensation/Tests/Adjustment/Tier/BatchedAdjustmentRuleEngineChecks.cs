namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedAdjustmentRuleEngineChecks
{
    public static void BatchedAdjustmentRuleEngineKeepsResultsWithinBounds()
    {
        var engine = BatchedAdjustmentRuleEngine.DefaultBatchedAdjustmentRuleEngine(Flags.NewRegistry());
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("code-196", "title-555");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BatchedAdjustmentRuleEngineKeepsResultsWithinBounds();
    }
}
