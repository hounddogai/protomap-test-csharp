namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyAuditEngineChecks
{
    public static void SeasonalPulseSurveyAuditEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPulseSurveyAuditEngine.DefaultSeasonalPulseSurveyAuditEngine();
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("code-436", "label-679");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyAuditEngineKeepsResultsWithinBounds();
    }
}
