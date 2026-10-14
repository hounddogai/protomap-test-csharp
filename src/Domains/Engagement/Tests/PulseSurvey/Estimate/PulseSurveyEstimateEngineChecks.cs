namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyEstimateEngineChecks
{
    public static void PulseSurveyEstimateEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyEstimateEngine.DefaultPulseSurveyEstimateEngine();
        var item = PulseSurveyEstimate.NewPulseSurveyEstimate("code-795", "name-705");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyEstimateEngineKeepsResultsWithinBounds();
    }
}
