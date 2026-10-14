namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Threshold;

using Acme.Workforce.Engagement.PulseSurvey.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class PulseSurveyThresholdEngineChecks
{
    public static void PulseSurveyThresholdEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyThresholdEngine.DefaultPulseSurveyThresholdEngine();
        var item = PulseSurveyThreshold.NewPulseSurveyThreshold("code-926", "label-997");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyThresholdEngineKeepsResultsWithinBounds();
    }
}
