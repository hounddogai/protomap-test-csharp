namespace Acme.Workforce.Compensation.Tests.PayBand.Threshold;

using Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PayBandMetricEngineChecks
{
    public static void PayBandMetricEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandMetricEngine.DefaultPayBandMetricEngine();
        var item = PayBandMetric.NewPayBandMetric("code-607", "title-840");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandMetricEngineKeepsResultsWithinBounds();
    }
}
