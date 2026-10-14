namespace Acme.Workforce.Workplace.Tests.Booking.Review;

using Acme.Workforce.Workplace.Booking.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class WeightedBookingEntryEngineChecks
{
    public static void WeightedBookingEntryEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedBookingEntryEngine.DefaultWeightedBookingEntryEngine();
        var item = WeightedBookingEntry.NewWeightedBookingEntry("code-213", "name-292");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedBookingEntryEngineKeepsResultsWithinBounds();
    }
}
