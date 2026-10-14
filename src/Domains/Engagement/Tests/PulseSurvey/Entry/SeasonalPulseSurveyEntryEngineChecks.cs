namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Entry;

using Acme.Workforce.Engagement.PulseSurvey.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyEntryEngineChecks
{
    public static void SeasonalPulseSurveyEntryEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalPulseSurveyEntryEngine.DefaultSeasonalPulseSurveyEntryEngine();
        var item = SeasonalPulseSurveyEntry.NewSeasonalPulseSurveyEntry("code-181", "label-858");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyEntryEngineKeepsResultsWithinBounds();
    }
}
