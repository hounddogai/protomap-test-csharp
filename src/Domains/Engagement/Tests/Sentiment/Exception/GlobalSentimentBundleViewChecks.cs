namespace Acme.Workforce.Engagement.Tests.Sentiment.Exception;

using Acme.Workforce.Engagement.Sentiment.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class GlobalSentimentBundleViewChecks
{
    public static void GlobalSentimentBundleViewShowsTheStatusInCapitals()
    {
        var item = GlobalSentimentBundle.NewGlobalSentimentBundle("code-374", "name-222");
        var view = GlobalSentimentBundleView.ToGlobalSentimentBundleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalSentimentBundleView.ToGlobalSentimentBundleViews(new List<GlobalSentimentBundle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalSentimentBundleViewShowsTheStatusInCapitals();
    }
}
