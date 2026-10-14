namespace Acme.Workforce.Assets.Tests.LicenseSeat.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LicenseSeatReservationEngineChecks
{
    public static void LicenseSeatReservationEngineKeepsResultsWithinBounds()
    {
        var engine = LicenseSeatReservationEngine.DefaultLicenseSeatReservationEngine();
        var item = LicenseSeatReservation.NewLicenseSeatReservation("code-999", "title-43");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LicenseSeatReservationEngineKeepsResultsWithinBounds();
    }
}
