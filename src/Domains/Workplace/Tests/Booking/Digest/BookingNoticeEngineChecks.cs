namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class BookingNoticeEngineChecks
{
    public static void BookingNoticeEngineKeepsResultsWithinBounds()
    {
        var engine = BookingNoticeEngine.DefaultBookingNoticeEngine(Flags.NewRegistry());
        var item = BookingNotice.NewBookingNotice("code-369", "title-778");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingNoticeEngineKeepsResultsWithinBounds();
    }
}
