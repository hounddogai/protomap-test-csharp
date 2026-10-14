namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class DefaultFeedbackReservationChecks
{
    public static void DefaultFeedbackReservationStartsInPlanned()
    {
        var item = DefaultFeedbackReservation.NewDefaultFeedbackReservation("code-135", "label-163");
        Check.Equal(item.Status, DefaultFeedbackReservationStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultFeedbackReservationMovesToCompleted()
    {
        var item = DefaultFeedbackReservation.NewDefaultFeedbackReservation("code-135", "label-163");
        try
        {
            item.MoveTo(DefaultFeedbackReservationStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultFeedbackReservationStatus.Completed, "status");
    }

    public static void DefaultFeedbackReservationRejectsMovesFromCancelled()
    {
        var item = DefaultFeedbackReservation.NewDefaultFeedbackReservation("code-135", "label-163");
        item.Status = DefaultFeedbackReservationStatus.Cancelled;
        Check.Fails(() => item.MoveTo(DefaultFeedbackReservationStatus.Planned), "move");
    }

    public static void RunAll()
    {
        DefaultFeedbackReservationStartsInPlanned();
        DefaultFeedbackReservationMovesToCompleted();
        DefaultFeedbackReservationRejectsMovesFromCancelled();
    }
}
