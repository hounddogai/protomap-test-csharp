namespace Acme.Workforce.Engagement.Tests.Sentiment.Reservation;

using Acme.Workforce.Engagement.Sentiment.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SentimentReservationEngineChecks
{
    public static void SentimentReservationEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentReservationEngine.DefaultSentimentReservationEngine();
        var item = SentimentReservation.NewSentimentReservation("code-695", "name-713");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentReservationEngineKeepsResultsWithinBounds();
    }
}
