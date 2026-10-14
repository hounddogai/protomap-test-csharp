namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class CappedPulseSurveyProfileEngineChecks
{
    public static void CappedPulseSurveyProfileEngineKeepsResultsWithinBounds()
    {
        var engine = CappedPulseSurveyProfileEngine.DefaultCappedPulseSurveyProfileEngine();
        var item = CappedPulseSurveyProfile.NewCappedPulseSurveyProfile("code-437", "title-372");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedPulseSurveyProfileEngineKeepsResultsWithinBounds();
    }
}
