namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Manifest;

using Acme.Workforce.Engagement.PulseSurvey.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class PulseSurveyRequestEngineChecks
{
    public static void PulseSurveyRequestEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyRequestEngine.DefaultPulseSurveyRequestEngine();
        var item = PulseSurveyRequest.NewPulseSurveyRequest("code-100", "title-13");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyRequestEngineKeepsResultsWithinBounds();
    }
}
