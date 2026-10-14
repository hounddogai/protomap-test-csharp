namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Preference;

using Acme.Workforce.Engagement.PulseSurvey.Preference;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class DefaultPulseSurveyOverrideEngineChecks
{
    public static void DefaultPulseSurveyOverrideEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultPulseSurveyOverrideEngine.DefaultDefaultPulseSurveyOverrideEngine(Flags.NewRegistry());
        var item = DefaultPulseSurveyOverride.NewDefaultPulseSurveyOverride("code-133", "name-793");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultPulseSurveyOverrideEngineKeepsResultsWithinBounds();
    }
}
