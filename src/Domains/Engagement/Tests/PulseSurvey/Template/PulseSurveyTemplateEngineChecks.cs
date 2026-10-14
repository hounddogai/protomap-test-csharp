namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Template;

using Acme.Workforce.Engagement.PulseSurvey.Template;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class PulseSurveyTemplateEngineChecks
{
    public static void PulseSurveyTemplateEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyTemplateEngine.DefaultPulseSurveyTemplateEngine(Flags.NewRegistry());
        var item = PulseSurveyTemplate.NewPulseSurveyTemplate("code-358", "label-299");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyTemplateEngineKeepsResultsWithinBounds();
    }
}
