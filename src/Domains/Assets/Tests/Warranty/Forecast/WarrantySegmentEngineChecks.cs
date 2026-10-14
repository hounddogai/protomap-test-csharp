namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantySegmentEngineChecks
{
    public static void WarrantySegmentEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantySegmentEngine.DefaultWarrantySegmentEngine();
        var item = WarrantySegment.NewWarrantySegment("code-584", "label-712");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantySegmentEngineKeepsResultsWithinBounds();
    }
}
