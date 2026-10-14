namespace Acme.Workforce.Engagement.Tests.Sentiment.Reservation;

using Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SentimentReservationViewChecks
{
    public static void SentimentReservationViewShowsTheStatusInCapitals()
    {
        var item = SentimentReservation.NewSentimentReservation("code-695", "name-713");
        var view = SentimentReservationView.ToSentimentReservationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentReservationView.ToSentimentReservationViews(new List<SentimentReservation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentReservationViewShowsTheStatusInCapitals();
    }
}
