namespace Acme.Workforce.Compensation.Tests.PayBand.Summary;

using Acme.Workforce.Compensation.PayBand.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class DefaultPayBandEstimateEngineChecks
{
    public static void DefaultPayBandEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPayBandEstimateEngine.DefaultDefaultPayBandEstimateEngine();
        var item = DefaultPayBandEstimate.NewDefaultPayBandEstimate("code-510", "title-297");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPayBandEstimateEngineKeepsResultsWithinBounds();
    }
}
