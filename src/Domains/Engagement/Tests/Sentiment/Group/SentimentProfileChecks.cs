namespace Acme.Workforce.Engagement.Tests.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SentimentProfileChecks
{
    public static void SentimentProfileStartsInDraft()
    {
        var item = SentimentProfile.NewSentimentProfile("code-989", "name-325");
        Check.Equal(item.Status, SentimentProfileStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentProfileMovesToClosed()
    {
        var item = SentimentProfile.NewSentimentProfile("code-989", "name-325");
        try
        {
            item.MoveTo(SentimentProfileStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentProfileStatus.Closed, "status");
    }

    public static void SentimentProfileRejectsMovesFromClosed()
    {
        var item = SentimentProfile.NewSentimentProfile("code-989", "name-325");
        item.Status = SentimentProfileStatus.Closed;
        Check.Fails(() => item.MoveTo(SentimentProfileStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SentimentProfileStartsInDraft();
        SentimentProfileMovesToClosed();
        SentimentProfileRejectsMovesFromClosed();
    }
}
