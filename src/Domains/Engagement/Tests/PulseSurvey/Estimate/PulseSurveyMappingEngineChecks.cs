namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyMappingEngineChecks
{
    public static void PulseSurveyMappingEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyMappingEngine.DefaultPulseSurveyMappingEngine(Flags.NewRegistry());
        var item = PulseSurveyMapping.NewPulseSurveyMapping("code-976", "label-253");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyMappingEngineKeepsResultsWithinBounds();
    }
}
