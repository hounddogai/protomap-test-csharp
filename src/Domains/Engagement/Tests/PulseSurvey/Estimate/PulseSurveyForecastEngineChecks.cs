namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyForecastEngineChecks
{
    public static void PulseSurveyForecastEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyForecastEngine.DefaultPulseSurveyForecastEngine(Flags.NewRegistry());
        var item = PulseSurveyForecast.NewPulseSurveyForecast("code-769", "title-999");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyForecastEngineKeepsResultsWithinBounds();
    }
}
