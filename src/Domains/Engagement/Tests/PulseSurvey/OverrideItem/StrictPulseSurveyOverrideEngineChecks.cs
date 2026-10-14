namespace Acme.Workforce.Engagement.Tests.PulseSurvey.OverrideItem;

using Acme.Workforce.Engagement.PulseSurvey.OverrideItem;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictPulseSurveyOverrideEngineChecks
{
    public static void StrictPulseSurveyOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = StrictPulseSurveyOverrideEngine.DefaultStrictPulseSurveyOverrideEngine(Flags.NewRegistry());
        var item = StrictPulseSurveyOverride.NewStrictPulseSurveyOverride("code-805", "label-768");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictPulseSurveyOverrideEngineKeepsResultsWithinBounds();
    }
}
