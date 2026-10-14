namespace Acme.Workforce.Compensation.Tests.PayBand.Draft;

using Acme.Workforce.Compensation.PayBand.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class PayBandEstimateEngineChecks
{
    public static void PayBandEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandEstimateEngine.DefaultPayBandEstimateEngine();
        var item = PayBandEstimate.NewPayBandEstimate("code-610", "label-938");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandEstimateEngineKeepsResultsWithinBounds();
    }
}
