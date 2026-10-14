namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyAudit;

using Acme.Workforce.Engagement.PulseSurvey.SurveyAudit;

/// <summary>Checks of survey audit code, run by the project's runner.</summary>
public static class GlobalPulseSurveyAdjustmentEngineChecks
{
    public static void GlobalPulseSurveyAdjustmentEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPulseSurveyAdjustmentEngine.DefaultGlobalPulseSurveyAdjustmentEngine();
        var item = GlobalPulseSurveyAdjustment.NewGlobalPulseSurveyAdjustment("code-881", "label-289");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPulseSurveyAdjustmentEngineKeepsResultsWithinBounds();
    }
}
