namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SentimentCycleChecks
{
    public static void SentimentCycleStartsInOpen()
    {
        var item = SentimentCycle.NewSentimentCycle("code-799", "name-898");
        Check.Equal(item.Status, SentimentCycleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentCycleMovesToReopened()
    {
        var item = SentimentCycle.NewSentimentCycle("code-799", "name-898");
        try
        {
            item.MoveTo(SentimentCycleStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentCycleStatus.Reopened, "status");
    }

    public static void SentimentCycleRejectsMovesFromArchived()
    {
        var item = SentimentCycle.NewSentimentCycle("code-799", "name-898");
        item.Status = SentimentCycleStatus.Archived;
        Check.Fails(() => item.MoveTo(SentimentCycleStatus.Open), "move");
    }

    public static void RunAll()
    {
        SentimentCycleStartsInOpen();
        SentimentCycleMovesToReopened();
        SentimentCycleRejectsMovesFromArchived();
    }
}
