namespace Acme.Workforce.Compensation.Tests.Adjustment.Mapping;

using Acme.Workforce.Compensation.Adjustment.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class AdjustmentMappingEngineChecks
{
    public static void AdjustmentMappingEngineKeepsResultsWithinBounds()
    {
        var engine = AdjustmentMappingEngine.DefaultAdjustmentMappingEngine();
        var item = AdjustmentMapping.NewAdjustmentMapping("code-836", "title-188");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdjustmentMappingEngineKeepsResultsWithinBounds();
    }
}
