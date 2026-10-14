namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class FeedbackReviewEngineChecks
{
    public static void FeedbackReviewEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackReviewEngine.DefaultFeedbackReviewEngine(Flags.NewRegistry());
        var item = FeedbackReview.NewFeedbackReview("code-194", "title-920");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackReviewEngineKeepsResultsWithinBounds();
    }
}
