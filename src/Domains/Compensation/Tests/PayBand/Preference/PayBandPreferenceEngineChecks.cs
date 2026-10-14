namespace Acme.Workforce.Compensation.Tests.PayBand.Preference;

using Acme.Workforce.Compensation.PayBand.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PayBandPreferenceEngineChecks
{
    public static void PayBandPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = PayBandPreferenceEngine.DefaultPayBandPreferenceEngine();
        var item = PayBandPreference.NewPayBandPreference("code-301", "title-777");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PayBandPreferenceEngineKeepsResultsWithinBounds();
    }
}
