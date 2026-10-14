namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of feedback reservation code, run by the project's runner.</summary>
public static class FallbackFeedbackReservationEngineChecks
{
    public static void FallbackFeedbackReservationEngineKeepsResultsWithinBounds()
    {
        var engine = FallbackFeedbackReservationEngine.DefaultFallbackFeedbackReservationEngine(Flags.NewRegistry());
        var item = FallbackFeedbackReservation.NewFallbackFeedbackReservation("code-694", "label-960");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FallbackFeedbackReservationEngineKeepsResultsWithinBounds();
    }
}
