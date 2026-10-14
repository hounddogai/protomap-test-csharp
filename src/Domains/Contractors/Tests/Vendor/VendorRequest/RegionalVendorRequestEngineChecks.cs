namespace Acme.Workforce.Contractors.Tests.Vendor.VendorRequest;

using Acme.Workforce.Contractors.Vendor.VendorRequest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of vendor request code, run by the project's runner.</summary>
public static class RegionalVendorRequestEngineChecks
{
    public static void RegionalVendorRequestEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalVendorRequestEngine.DefaultRegionalVendorRequestEngine(Flags.NewRegistry());
        var item = RegionalVendorRequest.NewRegionalVendorRequest("code-372", "title-6");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalVendorRequestEngineKeepsResultsWithinBounds();
    }
}
