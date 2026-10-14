namespace Acme.Workforce.Workplace.Tests.Booking.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BookingRequestEngineChecks
{
    public static void BookingRequestEngineKeepsResultsWithinBounds()
    {
        var engine = BookingRequestEngine.DefaultBookingRequestEngine(Flags.NewRegistry());
        var item = BookingRequest.NewBookingRequest("code-336", "title-371");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingRequestEngineKeepsResultsWithinBounds();
    }
}
