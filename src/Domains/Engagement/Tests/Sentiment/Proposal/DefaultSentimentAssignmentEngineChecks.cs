namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultSentimentAssignmentEngineChecks
{
    public static void DefaultSentimentAssignmentEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultSentimentAssignmentEngine.DefaultDefaultSentimentAssignmentEngine();
        var item = DefaultSentimentAssignment.NewDefaultSentimentAssignment("code-819", "title-577");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultSentimentAssignmentEngineKeepsResultsWithinBounds();
    }
}
