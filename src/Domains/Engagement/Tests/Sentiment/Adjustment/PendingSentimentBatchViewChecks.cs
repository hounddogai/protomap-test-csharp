namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingSentimentBatchViewChecks
{
    public static void PendingSentimentBatchViewShowsTheStatusInCapitals()
    {
        var item = PendingSentimentBatch.NewPendingSentimentBatch("code-343", "name-803");
        var view = PendingSentimentBatchView.ToPendingSentimentBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingSentimentBatchView.ToPendingSentimentBatchViews(new List<PendingSentimentBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingSentimentBatchViewShowsTheStatusInCapitals();
    }
}
