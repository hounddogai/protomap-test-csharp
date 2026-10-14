namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BookingForecastEngineChecks
{
    public static void BookingForecastEngineKeepsResultsWithinBounds()
    {
        var engine = BookingForecastEngine.DefaultBookingForecastEngine();
        var item = BookingForecast.NewBookingForecast("code-62", "title-438");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingForecastEngineKeepsResultsWithinBounds();
    }
}
