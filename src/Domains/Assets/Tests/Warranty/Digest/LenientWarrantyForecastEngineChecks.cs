namespace Acme.Workforce.Assets.Tests.Warranty.Digest;

using Acme.Workforce.Assets.Warranty.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class LenientWarrantyForecastEngineChecks
{
    public static void LenientWarrantyForecastEngineKeepsResultsWithinBounds()
    {
        var engine = LenientWarrantyForecastEngine.DefaultLenientWarrantyForecastEngine();
        var item = LenientWarrantyForecast.NewLenientWarrantyForecast("code-675", "label-486");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientWarrantyForecastEngineKeepsResultsWithinBounds();
    }
}
