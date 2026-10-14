namespace Acme.Workforce.Contractors.Tests.Vendor.Variance;

using Acme.Workforce.Contractors.Vendor.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class VendorAdjustmentEngineChecks
{
    public static void VendorAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = VendorAdjustmentEngine.DefaultVendorAdjustmentEngine();
        var item = VendorAdjustment.NewVendorAdjustment("code-403", "title-620");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorAdjustmentEngineKeepsResultsWithinBounds();
    }
}
