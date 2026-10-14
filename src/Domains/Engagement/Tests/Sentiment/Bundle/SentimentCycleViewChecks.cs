namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SentimentCycleViewChecks
{
    public static void SentimentCycleViewShowsTheStatusInCapitals()
    {
        var item = SentimentCycle.NewSentimentCycle("code-799", "name-898");
        var view = SentimentCycleView.ToSentimentCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentCycleView.ToSentimentCycleViews(new List<SentimentCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentCycleViewShowsTheStatusInCapitals();
    }
}
