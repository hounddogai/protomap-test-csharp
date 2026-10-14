namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class FeedbackExceptionEngineChecks
{
    public static void FeedbackExceptionEngineKeepsResultsWithinBounds()
    {
        var engine = FeedbackExceptionEngine.DefaultFeedbackExceptionEngine(Flags.NewRegistry());
        var item = FeedbackException.NewFeedbackException("code-106", "label-939");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FeedbackExceptionEngineKeepsResultsWithinBounds();
    }
}
