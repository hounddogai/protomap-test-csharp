namespace Acme.Workforce.Engagement.Tests.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SentimentProfileEngineChecks
{
    public static void SentimentProfileEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentProfileEngine.DefaultSentimentProfileEngine();
        var item = SentimentProfile.NewSentimentProfile("code-989", "name-325");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentProfileEngineKeepsResultsWithinBounds();
    }
}
