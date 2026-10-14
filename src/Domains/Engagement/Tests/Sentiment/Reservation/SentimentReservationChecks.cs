namespace Acme.Workforce.Engagement.Tests.Sentiment.Reservation;

using Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SentimentReservationChecks
{
    public static void SentimentReservationStartsInDraft()
    {
        var item = SentimentReservation.NewSentimentReservation("code-695", "name-713");
        Check.Equal(item.Status, SentimentReservationStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentReservationMovesToSuspended()
    {
        var item = SentimentReservation.NewSentimentReservation("code-695", "name-713");
        try
        {
            item.MoveTo(SentimentReservationStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentReservationStatus.Suspended, "status");
    }

    public static void SentimentReservationRejectsMovesFromClosed()
    {
        var item = SentimentReservation.NewSentimentReservation("code-695", "name-713");
        item.Status = SentimentReservationStatus.Closed;
        Check.Fails(() => item.MoveTo(SentimentReservationStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SentimentReservationStartsInDraft();
        SentimentReservationMovesToSuspended();
        SentimentReservationRejectsMovesFromClosed();
    }
}
