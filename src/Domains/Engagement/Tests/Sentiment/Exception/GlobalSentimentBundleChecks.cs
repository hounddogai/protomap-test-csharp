namespace Acme.Workforce.Engagement.Tests.Sentiment.Exception;

using Acme.Workforce.Engagement.Sentiment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class GlobalSentimentBundleChecks
{
    public static void GlobalSentimentBundleStartsInDraft()
    {
        var item = GlobalSentimentBundle.NewGlobalSentimentBundle("code-374", "name-222");
        Check.Equal(item.Status, GlobalSentimentBundleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalSentimentBundleMovesToPending()
    {
        var item = GlobalSentimentBundle.NewGlobalSentimentBundle("code-374", "name-222");
        try
        {
            item.MoveTo(GlobalSentimentBundleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalSentimentBundleStatus.Pending, "status");
    }

    public static void GlobalSentimentBundleRejectsMovesFromClosed()
    {
        var item = GlobalSentimentBundle.NewGlobalSentimentBundle("code-374", "name-222");
        item.Status = GlobalSentimentBundleStatus.Closed;
        Check.Fails(() => item.MoveTo(GlobalSentimentBundleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        GlobalSentimentBundleStartsInDraft();
        GlobalSentimentBundleMovesToPending();
        GlobalSentimentBundleRejectsMovesFromClosed();
    }
}
