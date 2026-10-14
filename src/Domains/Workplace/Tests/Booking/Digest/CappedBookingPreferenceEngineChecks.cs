namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CappedBookingPreferenceEngineChecks
{
    public static void CappedBookingPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = CappedBookingPreferenceEngine.DefaultCappedBookingPreferenceEngine();
        var item = CappedBookingPreference.NewCappedBookingPreference("code-162", "label-875");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedBookingPreferenceEngineKeepsResultsWithinBounds();
    }
}
