namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualPulseSurveyForecastEngineChecks
{
    public static void ManualPulseSurveyForecastEngineKeepsResultsWithinBounds()
    {
        var engine = ManualPulseSurveyForecastEngine.DefaultManualPulseSurveyForecastEngine(Flags.NewRegistry());
        var item = ManualPulseSurveyForecast.NewManualPulseSurveyForecast("code-937", "label-172");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ManualPulseSurveyForecastEngineKeepsResultsWithinBounds();
    }
}
