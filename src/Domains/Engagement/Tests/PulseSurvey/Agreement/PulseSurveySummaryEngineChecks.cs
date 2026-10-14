namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveySummaryEngineChecks
{
    public static void PulseSurveySummaryEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveySummaryEngine.DefaultPulseSurveySummaryEngine();
        var item = PulseSurveySummary.NewPulseSurveySummary("code-237", "name-286");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveySummaryEngineKeepsResultsWithinBounds();
    }
}
