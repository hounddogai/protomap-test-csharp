namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorManifestEngineChecks
{
    public static void VendorManifestEngineKeepsResultsWithinBounds()
    {
        var engine = VendorManifestEngine.DefaultVendorManifestEngine(Flags.NewRegistry());
        var item = VendorManifest.NewVendorManifest("code-44", "label-895");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorManifestEngineKeepsResultsWithinBounds();
    }
}
