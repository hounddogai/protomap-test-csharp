namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentPreferenceChecks
{
    public static void SentimentPreferenceStartsInDraft()
    {
        var item = SentimentPreference.NewSentimentPreference("code-911", "title-334");
        Check.Equal(item.Status, SentimentPreferenceStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentPreferenceMovesToActive()
    {
        var item = SentimentPreference.NewSentimentPreference("code-911", "title-334");
        try
        {
            item.MoveTo(SentimentPreferenceStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentPreferenceStatus.Active, "status");
    }

    public static void SentimentPreferenceRejectsMovesFromClosed()
    {
        var item = SentimentPreference.NewSentimentPreference("code-911", "title-334");
        item.Status = SentimentPreferenceStatus.Closed;
        Check.Fails(() => item.MoveTo(SentimentPreferenceStatus.Draft), "move");
    }

    public static void RunAll()
    {
        SentimentPreferenceStartsInDraft();
        SentimentPreferenceMovesToActive();
        SentimentPreferenceRejectsMovesFromClosed();
    }
}
