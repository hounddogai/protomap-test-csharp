namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>Checks of feedback reservation code, run by the project's runner.</summary>
public static class FallbackFeedbackReservationChecks
{
    public static void FallbackFeedbackReservationStartsInProposed()
    {
        var item = FallbackFeedbackReservation.NewFallbackFeedbackReservation("code-694", "label-960");
        Check.Equal(item.Status, FallbackFeedbackReservationStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackFeedbackReservationMovesToApproved()
    {
        var item = FallbackFeedbackReservation.NewFallbackFeedbackReservation("code-694", "label-960");
        try
        {
            item.MoveTo(FallbackFeedbackReservationStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackFeedbackReservationStatus.Approved, "status");
    }

    public static void FallbackFeedbackReservationRejectsMovesFromWithdrawn()
    {
        var item = FallbackFeedbackReservation.NewFallbackFeedbackReservation("code-694", "label-960");
        item.Status = FallbackFeedbackReservationStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(FallbackFeedbackReservationStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        FallbackFeedbackReservationStartsInProposed();
        FallbackFeedbackReservationMovesToApproved();
        FallbackFeedbackReservationRejectsMovesFromWithdrawn();
    }
}
