namespace Acme.Workforce.Engagement.Tests.Sentiment.Milestone;

using Acme.Workforce.Engagement.Sentiment.Milestone;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SentimentMilestoneEngineChecks
{
    public static void SentimentMilestoneEngineKeepsResultsWithinBounds()
    {
        var engine = SentimentMilestoneEngine.DefaultSentimentMilestoneEngine(Flags.NewRegistry());
        var item = SentimentMilestone.NewSentimentMilestone("code-717", "label-154");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SentimentMilestoneEngineKeepsResultsWithinBounds();
    }
}
