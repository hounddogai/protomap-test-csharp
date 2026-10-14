namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentAllocationViewChecks
{
    public static void SentimentAllocationViewShowsTheStatusInCapitals()
    {
        var item = SentimentAllocation.NewSentimentAllocation("code-746", "name-550");
        var view = SentimentAllocationView.ToSentimentAllocationView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentAllocationView.ToSentimentAllocationViews(new List<SentimentAllocation> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentAllocationViewShowsTheStatusInCapitals();
    }
}
