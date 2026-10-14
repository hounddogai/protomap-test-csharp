namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class LenientPulseSurveyClaimEngineChecks
{
    public static void LenientPulseSurveyClaimEngineKeepsResultsWithinBounds()
    {
        var engine = LenientPulseSurveyClaimEngine.DefaultLenientPulseSurveyClaimEngine();
        var item = LenientPulseSurveyClaim.NewLenientPulseSurveyClaim("code-727", "title-428");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientPulseSurveyClaimEngineKeepsResultsWithinBounds();
    }
}
