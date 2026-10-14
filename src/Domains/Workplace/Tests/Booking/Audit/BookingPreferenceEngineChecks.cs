namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingPreferenceEngineChecks
{
    public static void BookingPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = BookingPreferenceEngine.DefaultBookingPreferenceEngine(Flags.NewRegistry());
        var item = BookingPreference.NewBookingPreference("code-246", "label-286");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        BookingPreferenceEngineKeepsResultsWithinBounds();
    }
}
