namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class LegacyBookingVarianceEngineChecks
{
    public static void LegacyBookingVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyBookingVarianceEngine.DefaultLegacyBookingVarianceEngine(Flags.NewRegistry());
        var item = LegacyBookingVariance.NewLegacyBookingVariance("code-157", "name-577");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyBookingVarianceEngineKeepsResultsWithinBounds();
    }
}
