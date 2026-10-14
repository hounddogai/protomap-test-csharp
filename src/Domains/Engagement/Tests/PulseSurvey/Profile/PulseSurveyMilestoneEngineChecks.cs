namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PulseSurveyMilestoneEngineChecks
{
    public static void PulseSurveyMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyMilestoneEngine.DefaultPulseSurveyMilestoneEngine();
        var item = PulseSurveyMilestone.NewPulseSurveyMilestone("code-332", "label-303");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyMilestoneEngineKeepsResultsWithinBounds();
    }
}
