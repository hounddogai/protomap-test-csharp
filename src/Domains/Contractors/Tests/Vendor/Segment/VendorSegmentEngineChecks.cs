namespace Acme.Workforce.Contractors.Tests.Vendor.Segment;

using Acme.Workforce.Contractors.Vendor.Segment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class VendorSegmentEngineChecks
{
    public static void VendorSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = VendorSegmentEngine.DefaultVendorSegmentEngine(Flags.NewRegistry());
        var item = VendorSegment.NewVendorSegment("code-708", "title-369");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorSegmentEngineKeepsResultsWithinBounds();
    }
}
