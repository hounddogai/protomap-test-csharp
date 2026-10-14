namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BookingTierEngineChecks
{
    public static void BookingTierEngineKeepsResultsWithinBounds()
    {
        var engine = BookingTierEngine.DefaultBookingTierEngine(Flags.NewRegistry());
        var item = BookingTier.NewBookingTier("code-783", "name-449");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingTierEngineKeepsResultsWithinBounds();
    }
}
