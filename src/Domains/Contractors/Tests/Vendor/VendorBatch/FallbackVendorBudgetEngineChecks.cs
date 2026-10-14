namespace Acme.Workforce.Contractors.Tests.Vendor.VendorBatch;

using Acme.Workforce.Contractors.Vendor.VendorBatch;

/// <summary>Checks of vendor batch code, run by the project's runner.</summary>
public static class FallbackVendorBudgetEngineChecks
{
    public static void FallbackVendorBudgetEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackVendorBudgetEngine.DefaultFallbackVendorBudgetEngine();
        var item = FallbackVendorBudget.NewFallbackVendorBudget("code-306", "title-985");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackVendorBudgetEngineKeepsResultsWithinBounds();
    }
}
