namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class RollingPulseSurveyTemplateEngineChecks
{
    public static void RollingPulseSurveyTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = RollingPulseSurveyTemplateEngine.DefaultRollingPulseSurveyTemplateEngine(Flags.NewRegistry());
        var item = RollingPulseSurveyTemplate.NewRollingPulseSurveyTemplate("code-253", "title-119");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingPulseSurveyTemplateEngineKeepsResultsWithinBounds();
    }
}
