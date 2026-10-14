namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class GlobalBookingTierEngineChecks
{
    public static void GlobalBookingTierEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalBookingTierEngine.DefaultGlobalBookingTierEngine();
        var item = GlobalBookingTier.NewGlobalBookingTier("code-106", "title-494");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalBookingTierEngineKeepsResultsWithinBounds();
    }
}
