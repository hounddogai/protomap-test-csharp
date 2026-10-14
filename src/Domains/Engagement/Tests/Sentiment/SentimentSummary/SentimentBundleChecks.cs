namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentSummary;

using Acme.Workforce.Engagement.Sentiment.SentimentSummary;

/// <summary>Checks of sentiment summary code, run by the project's runner.</summary>
public static class SentimentBundleChecks
{
    public static void SentimentBundleStartsInCreated()
    {
        var item = SentimentBundle.NewSentimentBundle("code-423", "name-579");
        Check.Equal(item.Status, SentimentBundleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentBundleMovesToValidated()
    {
        var item = SentimentBundle.NewSentimentBundle("code-423", "name-579");
        try
        {
            item.MoveTo(SentimentBundleStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentBundleStatus.Validated, "status");
    }

    public static void SentimentBundleRejectsMovesFromRetired()
    {
        var item = SentimentBundle.NewSentimentBundle("code-423", "name-579");
        item.Status = SentimentBundleStatus.Retired;
        Check.Fails(() => item.MoveTo(SentimentBundleStatus.Created), "move");
    }

    public static void RunAll()
    {
        SentimentBundleStartsInCreated();
        SentimentBundleMovesToValidated();
        SentimentBundleRejectsMovesFromRetired();
    }
}
