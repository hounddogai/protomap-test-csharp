namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class WeightedBookingVarianceEngineChecks
{
    public static void WeightedBookingVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = WeightedBookingVarianceEngine.DefaultWeightedBookingVarianceEngine();
        var item = WeightedBookingVariance.NewWeightedBookingVariance("code-753", "name-226");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WeightedBookingVarianceEngineKeepsResultsWithinBounds();
    }
}
