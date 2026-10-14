namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Assignment;

using Acme.Workforce.Engagement.PulseSurvey.Assignment;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class PulseSurveyScoreEngineChecks
{
    public static void PulseSurveyScoreEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyScoreEngine.DefaultPulseSurveyScoreEngine(Flags.NewRegistry());
        var item = PulseSurveyScore.NewPulseSurveyScore("code-514", "name-829");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyScoreEngineKeepsResultsWithinBounds();
    }
}
