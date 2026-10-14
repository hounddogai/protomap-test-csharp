namespace Acme.Workforce.Assets.Tests.Laptop.Rollup;

using Acme.Workforce.Assets.Laptop.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class WeightedLaptopRollupEngineChecks
{
    public static void WeightedLaptopRollupEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedLaptopRollupEngine.DefaultWeightedLaptopRollupEngine(Flags.NewRegistry());
        var item = WeightedLaptopRollup.NewWeightedLaptopRollup("code-260", "name-745");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedLaptopRollupEngineKeepsResultsWithinBounds();
    }
}
