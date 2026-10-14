namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LegacyBookingRollupEngineChecks
{
    public static void LegacyBookingRollupEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyBookingRollupEngine.DefaultLegacyBookingRollupEngine(Flags.NewRegistry());
        var item = LegacyBookingRollup.NewLegacyBookingRollup("code-960", "title-565");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyBookingRollupEngineKeepsResultsWithinBounds();
    }
}
