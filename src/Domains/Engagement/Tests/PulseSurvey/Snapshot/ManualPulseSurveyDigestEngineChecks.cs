namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Snapshot;

using Acme.Workforce.Engagement.PulseSurvey.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ManualPulseSurveyDigestEngineChecks
{
    public static void ManualPulseSurveyDigestEngineKeepsResultsWithinBounds()
    {
        var engine = ManualPulseSurveyDigestEngine.DefaultManualPulseSurveyDigestEngine();
        var item = ManualPulseSurveyDigest.NewManualPulseSurveyDigest("code-970", "name-270");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualPulseSurveyDigestEngineKeepsResultsWithinBounds();
    }
}
