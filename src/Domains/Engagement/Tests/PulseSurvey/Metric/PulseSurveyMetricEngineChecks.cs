namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class PulseSurveyMetricEngineChecks
{
    public static void PulseSurveyMetricEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyMetricEngine.DefaultPulseSurveyMetricEngine();
        var item = PulseSurveyMetric.NewPulseSurveyMetric("code-663", "label-649");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyMetricEngineKeepsResultsWithinBounds();
    }
}
