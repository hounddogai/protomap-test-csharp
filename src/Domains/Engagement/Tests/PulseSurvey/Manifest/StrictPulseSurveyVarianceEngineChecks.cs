namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class StrictPulseSurveyVarianceEngineChecks
{
    public static void StrictPulseSurveyVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPulseSurveyVarianceEngine.DefaultStrictPulseSurveyVarianceEngine(Flags.NewRegistry());
        var item = StrictPulseSurveyVariance.NewStrictPulseSurveyVariance("code-104", "title-24");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPulseSurveyVarianceEngineKeepsResultsWithinBounds();
    }
}
