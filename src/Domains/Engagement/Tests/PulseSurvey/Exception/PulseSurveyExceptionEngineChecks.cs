namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Exception;

using Acme.Workforce.Engagement.PulseSurvey.Exception;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class PulseSurveyExceptionEngineChecks
{
    public static void PulseSurveyExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyExceptionEngine.DefaultPulseSurveyExceptionEngine(Flags.NewRegistry());
        var item = PulseSurveyException.NewPulseSurveyException("code-797", "title-704");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyExceptionEngineKeepsResultsWithinBounds();
    }
}
