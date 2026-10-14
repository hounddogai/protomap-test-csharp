namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatReservation;

using Acme.Workforce.Assets.LicenseSeat.SeatReservation;

/// <summary>Checks of seat reservation code, run by the project's runner.</summary>
public static class FallbackLicenseSeatReservationEngineChecks
{
    public static void FallbackLicenseSeatReservationEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackLicenseSeatReservationEngine.DefaultFallbackLicenseSeatReservationEngine();
        var item = FallbackLicenseSeatReservation.NewFallbackLicenseSeatReservation("code-285", "name-192");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackLicenseSeatReservationEngineKeepsResultsWithinBounds();
    }
}
