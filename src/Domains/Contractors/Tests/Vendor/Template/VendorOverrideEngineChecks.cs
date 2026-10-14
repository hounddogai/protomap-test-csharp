namespace Acme.Workforce.Contractors.Tests.Vendor.Template;

using Acme.Workforce.Contractors.Vendor.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class VendorOverrideEngineChecks
{
    public static void VendorOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = VendorOverrideEngine.DefaultVendorOverrideEngine();
        var item = VendorOverride.NewVendorOverride("code-841", "label-377");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorOverrideEngineKeepsResultsWithinBounds();
    }
}
