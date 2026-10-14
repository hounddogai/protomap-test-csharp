namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultPayBandReservationEngineChecks
{
    public static void DefaultPayBandReservationEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPayBandReservationEngine.DefaultDefaultPayBandReservationEngine();
        var item = DefaultPayBandReservation.NewDefaultPayBandReservation("code-140", "title-71");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPayBandReservationEngineKeepsResultsWithinBounds();
    }
}
