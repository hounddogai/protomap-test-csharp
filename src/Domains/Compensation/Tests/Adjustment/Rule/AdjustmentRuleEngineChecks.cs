namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentRuleEngineChecks
{
    public static void AdjustmentRuleEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentRuleEngine.DefaultAdjustmentRuleEngine();
        var item = AdjustmentRule.NewAdjustmentRule("code-929", "label-810");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentRuleEngineKeepsResultsWithinBounds();
    }
}
