namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingScoreEngineChecks
{
    public static void BookingScoreEngineKeepsResultsWithinBounds()
    {
        var engine = BookingScoreEngine.DefaultBookingScoreEngine(Flags.NewRegistry());
        var item = BookingScore.NewBookingScore("code-413", "label-301");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingScoreEngineKeepsResultsWithinBounds();
    }
}
