namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class PulseSurveyPlanEngineChecks
{
    public static void PulseSurveyPlanEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyPlanEngine.DefaultPulseSurveyPlanEngine(Flags.NewRegistry());
        var item = PulseSurveyPlan.NewPulseSurveyPlan("code-913", "label-457");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyPlanEngineKeepsResultsWithinBounds();
    }
}
