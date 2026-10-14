namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentAuditViewChecks
{
    public static void SentimentAuditViewShowsTheStatusInCapitals()
    {
        var item = SentimentAudit.NewSentimentAudit("code-735", "label-38");
        var view = SentimentAuditView.ToSentimentAuditView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentAuditView.ToSentimentAuditViews(new List<SentimentAudit> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentAuditViewShowsTheStatusInCapitals();
    }
}
