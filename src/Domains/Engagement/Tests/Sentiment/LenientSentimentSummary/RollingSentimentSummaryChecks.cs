namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class RollingSentimentSummaryChecks
{
    public static void RollingSentimentSummaryStartsInOpen()
    {
        var item = RollingSentimentSummary.NewRollingSentimentSummary("code-482", "name-279");
        Check.Equal(item.Status, RollingSentimentSummaryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingSentimentSummaryMovesToResolved()
    {
        var item = RollingSentimentSummary.NewRollingSentimentSummary("code-482", "name-279");
        try
        {
            item.MoveTo(RollingSentimentSummaryStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingSentimentSummaryStatus.Resolved, "status");
    }

    public static void RollingSentimentSummaryRejectsMovesFromArchived()
    {
        var item = RollingSentimentSummary.NewRollingSentimentSummary("code-482", "name-279");
        item.Status = RollingSentimentSummaryStatus.Archived;
        Check.Fails(() => item.MoveTo(RollingSentimentSummaryStatus.Open), "move");
    }

    public static void RunAll()
    {
        RollingSentimentSummaryStartsInOpen();
        RollingSentimentSummaryMovesToResolved();
        RollingSentimentSummaryRejectsMovesFromArchived();
    }
}
