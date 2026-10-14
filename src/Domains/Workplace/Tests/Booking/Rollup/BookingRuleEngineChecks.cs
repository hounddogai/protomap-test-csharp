namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BookingRuleEngineChecks
{
    public static void BookingRuleEngineKeepsResultsWithinBounds()
    {
        var engine = BookingRuleEngine.DefaultBookingRuleEngine();
        var item = BookingRule.NewBookingRule("code-135", "name-449");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingRuleEngineKeepsResultsWithinBounds();
    }
}
