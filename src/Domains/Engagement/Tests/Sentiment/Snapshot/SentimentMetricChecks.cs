namespace Acme.Workforce.Engagement.Tests.Sentiment.Snapshot;

using Acme.Workforce.Engagement.Sentiment.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SentimentMetricChecks
{
    public static void SentimentMetricStartsInRequested()
    {
        var item = SentimentMetric.NewSentimentMetric("code-774", "name-977");
        Check.Equal(item.Status, SentimentMetricStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentMetricMovesToExpired()
    {
        var item = SentimentMetric.NewSentimentMetric("code-774", "name-977");
        try
        {
            item.MoveTo(SentimentMetricStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentMetricStatus.Expired, "status");
    }

    public static void SentimentMetricRejectsMovesFromExpired()
    {
        var item = SentimentMetric.NewSentimentMetric("code-774", "name-977");
        item.Status = SentimentMetricStatus.Expired;
        Check.Fails(() => item.MoveTo(SentimentMetricStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SentimentMetricStartsInRequested();
        SentimentMetricMovesToExpired();
        SentimentMetricRejectsMovesFromExpired();
    }
}
