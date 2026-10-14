namespace Acme.Workforce.Assets.Tests.Warranty.Ledger;

using Acme.Workforce.Assets.Warranty.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AdaptiveWarrantyMappingEngineChecks
{
    public static void AdaptiveWarrantyMappingEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveWarrantyMappingEngine.DefaultAdaptiveWarrantyMappingEngine();
        var item = AdaptiveWarrantyMapping.NewAdaptiveWarrantyMapping("code-248", "label-211");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveWarrantyMappingEngineKeepsResultsWithinBounds();
    }
}
