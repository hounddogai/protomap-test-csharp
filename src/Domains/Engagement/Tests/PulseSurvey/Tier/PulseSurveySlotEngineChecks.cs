namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Tier;

using Acme.Workforce.Engagement.PulseSurvey.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class PulseSurveySlotEngineChecks
{
    public static void PulseSurveySlotEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveySlotEngine.DefaultPulseSurveySlotEngine();
        var item = PulseSurveySlot.NewPulseSurveySlot("code-628", "name-637");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveySlotEngineKeepsResultsWithinBounds();
    }
}
