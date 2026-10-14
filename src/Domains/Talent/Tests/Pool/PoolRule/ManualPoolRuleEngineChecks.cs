namespace Acme.Workforce.Talent.Tests.Pool.PoolRule;

using Acme.Workforce.Talent.Pool.PoolRule;

/// <summary>Checks of pool rule code, run by the project's runner.</summary>
public static class ManualPoolRuleEngineChecks
{
    public static void ManualPoolRuleEngineKeepsResultsWithinBounds()
    {
        var engine = ManualPoolRuleEngine.DefaultManualPoolRuleEngine();
        var item = ManualPoolRule.NewManualPoolRule("code-350", "name-673");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualPoolRuleEngineKeepsResultsWithinBounds();
    }
}
