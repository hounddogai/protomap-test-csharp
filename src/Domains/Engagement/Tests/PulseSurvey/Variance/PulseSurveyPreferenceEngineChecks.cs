namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Variance;

using Acme.Workforce.Engagement.PulseSurvey.Variance;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PulseSurveyPreferenceEngineChecks
{
    public static void PulseSurveyPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = PulseSurveyPreferenceEngine.DefaultPulseSurveyPreferenceEngine(Flags.NewRegistry());
        var item = PulseSurveyPreference.NewPulseSurveyPreference("code-365", "label-138");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        PulseSurveyPreferenceEngineKeepsResultsWithinBounds();
    }
}
