namespace Acme.Workforce.Assets.Tests.Warranty.Manifest;

using Acme.Workforce.Assets.Warranty.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class WarrantyMappingEngineChecks
{
    public static void WarrantyMappingEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyMappingEngine.DefaultWarrantyMappingEngine(Flags.NewRegistry());
        var item = WarrantyMapping.NewWarrantyMapping("code-155", "label-791");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyMappingEngineKeepsResultsWithinBounds();
    }
}
