namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TieredFeedbackPreferenceEngineChecks
{
    public static void TieredFeedbackPreferenceEngineKeepsResultsWithinBounds()
    {
        var engine = TieredFeedbackPreferenceEngine.DefaultTieredFeedbackPreferenceEngine();
        var item = TieredFeedbackPreference.NewTieredFeedbackPreference("code-430", "name-420");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredFeedbackPreferenceEngineKeepsResultsWithinBounds();
    }
}
