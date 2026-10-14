namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentRuleViewChecks
{
    public static void SentimentRuleViewShowsTheStatusInCapitals()
    {
        var item = SentimentRule.NewSentimentRule("code-271", "name-872");
        var view = SentimentRuleView.ToSentimentRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentRuleView.ToSentimentRuleViews(new List<SentimentRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentRuleViewShowsTheStatusInCapitals();
    }
}
