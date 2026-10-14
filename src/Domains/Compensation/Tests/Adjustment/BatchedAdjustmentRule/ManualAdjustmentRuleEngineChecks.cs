namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class ManualAdjustmentRuleEngineChecks
{
    public static void ManualAdjustmentRuleEngineKeepsResultsWithinBounds()
    {
        var engine = ManualAdjustmentRuleEngine.DefaultManualAdjustmentRuleEngine();
        var item = ManualAdjustmentRule.NewManualAdjustmentRule("code-770", "title-367");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualAdjustmentRuleEngineKeepsResultsWithinBounds();
    }
}
