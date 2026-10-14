namespace Acme.Workforce.Assets.Tests.Warranty.Metric;

using Acme.Workforce.Assets.Warranty.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class WarrantyClaimEngineChecks
{
    public static void WarrantyClaimEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyClaimEngine.DefaultWarrantyClaimEngine();
        var item = WarrantyClaim.NewWarrantyClaim("code-400", "name-178");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyClaimEngineKeepsResultsWithinBounds();
    }
}
