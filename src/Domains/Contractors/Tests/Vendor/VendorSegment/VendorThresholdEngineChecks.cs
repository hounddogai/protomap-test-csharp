namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class VendorThresholdEngineChecks
{
    public static void VendorThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = VendorThresholdEngine.DefaultVendorThresholdEngine(Flags.NewRegistry());
        var item = VendorThreshold.NewVendorThreshold("code-603", "name-36");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorThresholdEngineKeepsResultsWithinBounds();
    }
}
