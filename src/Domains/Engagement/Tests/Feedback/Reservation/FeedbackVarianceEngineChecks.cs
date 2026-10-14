namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FeedbackVarianceEngineChecks
{
    public static void FeedbackVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackVarianceEngine.DefaultFeedbackVarianceEngine(Flags.NewRegistry());
        var item = FeedbackVariance.NewFeedbackVariance("code-711", "title-123");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackVarianceEngineKeepsResultsWithinBounds();
    }
}
