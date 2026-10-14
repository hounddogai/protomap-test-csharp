namespace Acme.Workforce.Assets.Tests.Warranty.WarrantyAdjustment;

using Acme.Workforce.Assets.Warranty.WarrantyAdjustment;

/// <summary>Checks of warranty adjustment code, run by the project's runner.</summary>
public static class WeightedWarrantyMappingEngineChecks
{
    public static void WeightedWarrantyMappingEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedWarrantyMappingEngine.DefaultWeightedWarrantyMappingEngine();
        var item = WeightedWarrantyMapping.NewWeightedWarrantyMapping("code-496", "label-839");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedWarrantyMappingEngineKeepsResultsWithinBounds();
    }
}
