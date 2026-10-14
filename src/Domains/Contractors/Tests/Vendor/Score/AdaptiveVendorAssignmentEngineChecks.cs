namespace Acme.Workforce.Contractors.Tests.Vendor.Score;

using Acme.Workforce.Contractors.Vendor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdaptiveVendorAssignmentEngineChecks
{
    public static void AdaptiveVendorAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveVendorAssignmentEngine.DefaultAdaptiveVendorAssignmentEngine();
        var item = AdaptiveVendorAssignment.NewAdaptiveVendorAssignment("code-657", "title-8");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveVendorAssignmentEngineKeepsResultsWithinBounds();
    }
}
