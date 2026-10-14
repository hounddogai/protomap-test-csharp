namespace Acme.Workforce.Engagement.Tests.Sentiment.LenientSentimentSummary;

using Acme.Workforce.Engagement.Sentiment.LenientSentimentSummary;

/// <summary>Checks of lenient sentiment summary code, run by the project's runner.</summary>
public static class SentimentTierChecks
{
    public static void SentimentTierStartsInCreated()
    {
        var item = SentimentTier.NewSentimentTier("code-167", "name-928");
        Check.Equal(item.Status, SentimentTierStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentTierMovesToValidated()
    {
        var item = SentimentTier.NewSentimentTier("code-167", "name-928");
        try
        {
            item.MoveTo(SentimentTierStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentTierStatus.Validated, "status");
    }

    public static void SentimentTierRejectsMovesFromRetired()
    {
        var item = SentimentTier.NewSentimentTier("code-167", "name-928");
        item.Status = SentimentTierStatus.Retired;
        Check.Fails(() => item.MoveTo(SentimentTierStatus.Created), "move");
    }

    public static void RunAll()
    {
        SentimentTierStartsInCreated();
        SentimentTierMovesToValidated();
        SentimentTierRejectsMovesFromRetired();
    }
}
