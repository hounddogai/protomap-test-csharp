namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class AdaptiveFeedbackPreferenceEngineChecks
{
    public static void AdaptiveFeedbackPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = AdaptiveFeedbackPreferenceEngine.DefaultAdaptiveFeedbackPreferenceEngine(Flags.NewRegistry());
        var item = AdaptiveFeedbackPreference.NewAdaptiveFeedbackPreference("code-850", "name-39");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        AdaptiveFeedbackPreferenceEngineKeepsResultsWithinBounds();
    }
}
