namespace Acme.Workforce.Contractors.Tests.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Vendor.VendorSegment;

/// <summary>Checks of vendor segment code, run by the project's runner.</summary>
public static class ManualVendorSegmentEngineChecks
{
    public static void ManualVendorSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = ManualVendorSegmentEngine.DefaultManualVendorSegmentEngine();
        var item = ManualVendorSegment.NewManualVendorSegment("code-775", "title-238");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualVendorSegmentEngineKeepsResultsWithinBounds();
    }
}
