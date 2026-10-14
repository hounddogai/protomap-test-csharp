namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class PayBandRequestEngineChecks
{
    public static void PayBandRequestEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandRequestEngine.DefaultPayBandRequestEngine(Flags.NewRegistry());
        var item = PayBandRequest.NewPayBandRequest("code-593", "name-25");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandRequestEngineKeepsResultsWithinBounds();
    }
}
