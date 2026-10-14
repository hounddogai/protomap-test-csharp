namespace Acme.Workforce.Contractors.Tests.Vendor.Review;

using Acme.Workforce.Contractors.Vendor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class VendorRequestEngineChecks
{
    public static void VendorRequestEngineKeepsResultsWithinBounds()
    {
        var engine = VendorRequestEngine.DefaultVendorRequestEngine();
        var item = VendorRequest.NewVendorRequest("code-556", "label-912");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        VendorRequestEngineKeepsResultsWithinBounds();
    }
}
