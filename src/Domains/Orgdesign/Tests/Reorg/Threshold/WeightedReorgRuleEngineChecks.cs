namespace Acme.Workforce.Orgdesign.Tests.Reorg.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Orgdesign.Reorg.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class WeightedReorgRuleEngineChecks
{
    public static void WeightedReorgRuleEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedReorgRuleEngine.DefaultWeightedReorgRuleEngine(Flags.NewRegistry());
        var item = WeightedReorgRule.NewWeightedReorgRule("code-370", "name-700");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedReorgRuleEngineKeepsResultsWithinBounds();
    }
}
