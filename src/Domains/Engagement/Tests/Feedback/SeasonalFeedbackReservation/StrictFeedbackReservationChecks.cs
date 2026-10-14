namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class StrictFeedbackReservationChecks
{
    public static void StrictFeedbackReservationStartsInOpen()
    {
        var item = StrictFeedbackReservation.NewStrictFeedbackReservation("code-295", "label-661");
        Check.Equal(item.Status, StrictFeedbackReservationStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictFeedbackReservationMovesToInReview()
    {
        var item = StrictFeedbackReservation.NewStrictFeedbackReservation("code-295", "label-661");
        try
        {
            item.MoveTo(StrictFeedbackReservationStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictFeedbackReservationStatus.InReview, "status");
    }

    public static void StrictFeedbackReservationRejectsMovesFromArchived()
    {
        var item = StrictFeedbackReservation.NewStrictFeedbackReservation("code-295", "label-661");
        item.Status = StrictFeedbackReservationStatus.Archived;
        Check.Fails(() => item.MoveTo(StrictFeedbackReservationStatus.Open), "move");
    }

    public static void RunAll()
    {
        StrictFeedbackReservationStartsInOpen();
        StrictFeedbackReservationMovesToInReview();
        StrictFeedbackReservationRejectsMovesFromArchived();
    }
}
