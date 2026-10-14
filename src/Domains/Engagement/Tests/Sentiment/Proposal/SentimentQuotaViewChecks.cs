namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentQuotaViewChecks
{
    public static void SentimentQuotaViewShowsTheStatusInCapitals()
    {
        var item = SentimentQuota.NewSentimentQuota("code-492", "name-741");
        var view = SentimentQuotaView.ToSentimentQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentQuotaView.ToSentimentQuotaViews(new List<SentimentQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentQuotaViewShowsTheStatusInCapitals();
    }
}
