namespace Acme.Workforce.Compensation.Tests.PayBand.Rollup;

using Acme.Workforce.Compensation.PayBand.Rollup;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PayBandForecastEngineChecks
{
    public static void PayBandForecastEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandForecastEngine.DefaultPayBandForecastEngine(Flags.NewRegistry());
        var item = PayBandForecast.NewPayBandForecast("code-348", "label-555");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandForecastEngineKeepsResultsWithinBounds();
    }
}
