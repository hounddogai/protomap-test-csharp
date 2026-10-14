namespace Acme.Workforce.Talent.Tests.Mobility.OverrideItem;

using Acme.Workforce.Talent.Mobility.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class WeightedMobilityRollupEngineChecks
{
    public static void WeightedMobilityRollupEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedMobilityRollupEngine.DefaultWeightedMobilityRollupEngine();
        var item = WeightedMobilityRollup.NewWeightedMobilityRollup("code-599", "name-549");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedMobilityRollupEngineKeepsResultsWithinBounds();
    }
}
