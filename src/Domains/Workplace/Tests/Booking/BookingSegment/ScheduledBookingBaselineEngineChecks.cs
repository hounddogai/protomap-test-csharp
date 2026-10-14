namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class ScheduledBookingBaselineEngineChecks
{
    public static void ScheduledBookingBaselineEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledBookingBaselineEngine.DefaultScheduledBookingBaselineEngine();
        var item = ScheduledBookingBaseline.NewScheduledBookingBaseline("code-852", "title-869");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledBookingBaselineEngineKeepsResultsWithinBounds();
    }
}
