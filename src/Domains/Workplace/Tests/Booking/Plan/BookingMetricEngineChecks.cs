namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingMetricEngineChecks
{
    public static void BookingMetricEngineKeepsResultsWithinBounds()
    {
        var engine = BookingMetricEngine.DefaultBookingMetricEngine();
        var item = BookingMetric.NewBookingMetric("code-748", "name-865");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingMetricEngineKeepsResultsWithinBounds();
    }
}
