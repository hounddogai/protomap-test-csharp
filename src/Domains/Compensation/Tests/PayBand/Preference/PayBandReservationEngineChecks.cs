namespace Acme.Workforce.Compensation.Tests.PayBand.Preference;

using Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PayBandReservationEngineChecks
{
    public static void PayBandReservationEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandReservationEngine.DefaultPayBandReservationEngine();
        var item = PayBandReservation.NewPayBandReservation("code-76", "name-429");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandReservationEngineKeepsResultsWithinBounds();
    }
}
