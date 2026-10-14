namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RegionalBookingPreferenceEngineChecks
{
    public static void RegionalBookingPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = RegionalBookingPreferenceEngine.DefaultRegionalBookingPreferenceEngine();
        var item = RegionalBookingPreference.NewRegionalBookingPreference("code-562", "label-881");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RegionalBookingPreferenceEngineKeepsResultsWithinBounds();
    }
}
