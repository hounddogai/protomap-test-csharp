namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyRecordEngineChecks
{
    public static void PulseSurveyRecordEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyRecordEngine.DefaultPulseSurveyRecordEngine(Flags.NewRegistry());
        var item = PulseSurveyRecord.NewPulseSurveyRecord("code-936", "name-423");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyRecordEngineKeepsResultsWithinBounds();
    }
}
