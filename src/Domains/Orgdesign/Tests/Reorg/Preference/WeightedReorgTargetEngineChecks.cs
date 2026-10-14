namespace Acme.Workforce.Orgdesign.Tests.Reorg.Preference;

using Acme.Workforce.Orgdesign.Reorg.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class WeightedReorgTargetEngineChecks
{
    public static void WeightedReorgTargetEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedReorgTargetEngine.DefaultWeightedReorgTargetEngine();
        var item = WeightedReorgTarget.NewWeightedReorgTarget("code-324", "label-262");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedReorgTargetEngineKeepsResultsWithinBounds();
    }
}
