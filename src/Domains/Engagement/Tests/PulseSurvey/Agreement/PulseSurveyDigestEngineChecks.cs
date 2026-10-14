namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Agreement;

using Acme.Workforce.Engagement.PulseSurvey.Agreement;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class PulseSurveyDigestEngineChecks
{
    public static void PulseSurveyDigestEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyDigestEngine.DefaultPulseSurveyDigestEngine(Flags.NewRegistry());
        var item = PulseSurveyDigest.NewPulseSurveyDigest("code-388", "title-805");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyDigestEngineKeepsResultsWithinBounds();
    }
}
