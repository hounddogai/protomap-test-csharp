namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingProfileEngineChecks
{
    public static void BookingProfileEngineKeepsResultsWithinBounds()
    {
        var engine = BookingProfileEngine.DefaultBookingProfileEngine(Flags.NewRegistry());
        var item = BookingProfile.NewBookingProfile("code-249", "label-779");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingProfileEngineKeepsResultsWithinBounds();
    }
}
