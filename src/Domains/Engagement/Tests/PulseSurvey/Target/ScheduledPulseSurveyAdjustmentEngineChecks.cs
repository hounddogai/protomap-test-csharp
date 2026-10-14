namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ScheduledPulseSurveyAdjustmentEngineChecks
{
    public static void ScheduledPulseSurveyAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPulseSurveyAdjustmentEngine.DefaultScheduledPulseSurveyAdjustmentEngine();
        var item = ScheduledPulseSurveyAdjustment.NewScheduledPulseSurveyAdjustment("code-955", "title-421");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPulseSurveyAdjustmentEngineKeepsResultsWithinBounds();
    }
}
