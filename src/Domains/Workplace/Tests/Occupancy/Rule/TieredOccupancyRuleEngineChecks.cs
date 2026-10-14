namespace Acme.Workforce.Workplace.Tests.Occupancy.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Occupancy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TieredOccupancyRuleEngineChecks
{
    public static void TieredOccupancyRuleEngineKeepsResultsWithinBounds()
    {
        var engine = TieredOccupancyRuleEngine.DefaultTieredOccupancyRuleEngine(Flags.NewRegistry());
        var item = TieredOccupancyRule.NewTieredOccupancyRule("code-702", "label-675");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredOccupancyRuleEngineKeepsResultsWithinBounds();
    }
}
