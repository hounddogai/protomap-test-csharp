namespace Acme.Workforce.Workplace.Tests.Booking.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BookingEntryEngineChecks
{
    public static void BookingEntryEngineKeepsResultsWithinBounds()
    {
        var engine = BookingEntryEngine.DefaultBookingEntryEngine(Flags.NewRegistry());
        var item = BookingEntry.NewBookingEntry("code-623", "title-581");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingEntryEngineKeepsResultsWithinBounds();
    }
}
