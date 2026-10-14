namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class SeasonalBookingSegmentEngineChecks
{
    public static void SeasonalBookingSegmentEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalBookingSegmentEngine.DefaultSeasonalBookingSegmentEngine(Flags.NewRegistry());
        var item = SeasonalBookingSegment.NewSeasonalBookingSegment("code-404", "name-512");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalBookingSegmentEngineKeepsResultsWithinBounds();
    }
}
