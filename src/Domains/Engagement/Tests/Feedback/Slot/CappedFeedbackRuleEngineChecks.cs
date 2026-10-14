namespace Acme.Workforce.Engagement.Tests.Feedback.Slot;

using Acme.Workforce.Engagement.Feedback.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CappedFeedbackRuleEngineChecks
{
    public static void CappedFeedbackRuleEngineKeepsResultsWithinBounds()
    {
        var engine = CappedFeedbackRuleEngine.DefaultCappedFeedbackRuleEngine();
        var item = CappedFeedbackRule.NewCappedFeedbackRule("code-934", "label-501");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        CappedFeedbackRuleEngineKeepsResultsWithinBounds();
    }
}
