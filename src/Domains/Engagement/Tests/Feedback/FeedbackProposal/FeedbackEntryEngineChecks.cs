namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class FeedbackEntryEngineChecks
{
    public static void FeedbackEntryEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackEntryEngine.DefaultFeedbackEntryEngine();
        var item = FeedbackEntry.NewFeedbackEntry("code-51", "label-215");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackEntryEngineKeepsResultsWithinBounds();
    }
}
