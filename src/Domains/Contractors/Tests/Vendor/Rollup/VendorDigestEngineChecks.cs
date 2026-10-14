namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class VendorDigestEngineChecks
{
    public static void VendorDigestEngineKeepsResultsWithinBounds()
    {
        var engine = VendorDigestEngine.DefaultVendorDigestEngine();
        var item = VendorDigest.NewVendorDigest("code-744", "name-942");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorDigestEngineKeepsResultsWithinBounds();
    }
}
