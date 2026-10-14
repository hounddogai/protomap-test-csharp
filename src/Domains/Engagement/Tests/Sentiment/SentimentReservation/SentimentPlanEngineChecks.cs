namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentReservation;

using Acme.Workforce.Engagement.Sentiment.SentimentReservation;

/// <summary>Checks of sentiment reservation code, run by the project's runner.</summary>
public static class SentimentPlanEngineChecks
{
    public static void SentimentPlanEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentPlanEngine.DefaultSentimentPlanEngine();
        var item = SentimentPlan.NewSentimentPlan("code-375", "label-994");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentPlanEngineKeepsResultsWithinBounds();
    }
}
