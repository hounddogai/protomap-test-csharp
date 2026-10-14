namespace Acme.Workforce.Contractors.Tests.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Vendor.VendorMetric;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of vendor metric code, run by the project's runner.</summary>
public static class VendorMappingEngineChecks
{
    public static void VendorMappingEngineKeepsResultsWithinBounds()
    {
        var engine = VendorMappingEngine.DefaultVendorMappingEngine(Flags.NewRegistry());
        var item = VendorMapping.NewVendorMapping("code-274", "title-517");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorMappingEngineKeepsResultsWithinBounds();
    }
}
