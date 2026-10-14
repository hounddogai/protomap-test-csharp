namespace Acme.Workforce.Workplace.Tests.Booking.BookingReservation;

using Acme.Workforce.Workplace.Booking.BookingReservation;

/// <summary>Checks of booking reservation code, run by the project's runner.</summary>
public static class RegionalBookingReservationEngineChecks
{
    public static void RegionalBookingReservationEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalBookingReservationEngine.DefaultRegionalBookingReservationEngine();
        var item = RegionalBookingReservation.NewRegionalBookingReservation("code-680", "name-694");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalBookingReservationEngineKeepsResultsWithinBounds();
    }
}
