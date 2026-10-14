namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalWarrantyDigestEngineChecks
{
    public static void SeasonalWarrantyDigestEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalWarrantyDigestEngine.DefaultSeasonalWarrantyDigestEngine();
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("code-946", "title-206");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalWarrantyDigestEngineKeepsResultsWithinBounds();
    }
}
