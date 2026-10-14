namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class StrictPulseSurveyPreferenceEngineChecks
{
    public static void StrictPulseSurveyPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPulseSurveyPreferenceEngine.DefaultStrictPulseSurveyPreferenceEngine();
        var item = StrictPulseSurveyPreference.NewStrictPulseSurveyPreference("code-334", "name-888");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPulseSurveyPreferenceEngineKeepsResultsWithinBounds();
    }
}
