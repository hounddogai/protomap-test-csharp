namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackSentimentBundleChecks
{
    public static void FallbackSentimentBundleStartsInOpen()
    {
        var item = FallbackSentimentBundle.NewFallbackSentimentBundle("code-293", "title-789");
        Check.Equal(item.Status, FallbackSentimentBundleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackSentimentBundleMovesToArchived()
    {
        var item = FallbackSentimentBundle.NewFallbackSentimentBundle("code-293", "title-789");
        try
        {
            item.MoveTo(FallbackSentimentBundleStatus.Archived);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackSentimentBundleStatus.Archived, "status");
    }

    public static void FallbackSentimentBundleRejectsMovesFromArchived()
    {
        var item = FallbackSentimentBundle.NewFallbackSentimentBundle("code-293", "title-789");
        item.Status = FallbackSentimentBundleStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackSentimentBundleStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackSentimentBundleStartsInOpen();
        FallbackSentimentBundleMovesToArchived();
        FallbackSentimentBundleRejectsMovesFromArchived();
    }
}
