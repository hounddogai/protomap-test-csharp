namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PrimaryBookingWindowEngineChecks
{
    public static void PrimaryBookingWindowEngineKeepsResultsWithinBounds()
    {
        var engine = PrimaryBookingWindowEngine.DefaultPrimaryBookingWindowEngine();
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("code-997", "name-788");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PrimaryBookingWindowEngineKeepsResultsWithinBounds();
    }
}
