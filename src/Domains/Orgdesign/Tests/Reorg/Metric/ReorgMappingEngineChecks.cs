namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgMappingEngineChecks
{
    public static void ReorgMappingEngineKeepsResultsWithinBounds()
    {
        var engine = ReorgMappingEngine.DefaultReorgMappingEngine();
        var item = ReorgMapping.NewReorgMapping("code-799", "name-176");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReorgMappingEngineKeepsResultsWithinBounds();
    }
}
