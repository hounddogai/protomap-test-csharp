namespace Acme.Workforce.Contractors.Tests.Vendor.Rollup;

using Acme.Workforce.Contractors.Vendor.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class GlobalVendorReviewEngineChecks
{
    public static void GlobalVendorReviewEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalVendorReviewEngine.DefaultGlobalVendorReviewEngine();
        var item = GlobalVendorReview.NewGlobalVendorReview("code-830", "name-195");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalVendorReviewEngineKeepsResultsWithinBounds();
    }
}
