namespace Acme.Workforce.Engagement.Tests.PulseSurvey.RecordItem;

using Acme.Workforce.Engagement.PulseSurvey.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class GlobalPulseSurveyRecordEngineChecks
{
    public static void GlobalPulseSurveyRecordEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalPulseSurveyRecordEngine.DefaultGlobalPulseSurveyRecordEngine();
        var item = GlobalPulseSurveyRecord.NewGlobalPulseSurveyRecord("code-931", "name-823");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalPulseSurveyRecordEngineKeepsResultsWithinBounds();
    }
}
