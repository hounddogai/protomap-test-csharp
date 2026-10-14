namespace Acme.Workforce.Engagement.Tests.Sentiment.Manifest;

using Acme.Workforce.Engagement.Sentiment.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledSentimentTargetChecks
{
    public static void ScheduledSentimentTargetStartsInIdle()
    {
        var item = ScheduledSentimentTarget.NewScheduledSentimentTarget("code-371", "name-460");
        Check.Equal(item.Status, ScheduledSentimentTargetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledSentimentTargetMovesToWarming()
    {
        var item = ScheduledSentimentTarget.NewScheduledSentimentTarget("code-371", "name-460");
        try
        {
            item.MoveTo(ScheduledSentimentTargetStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledSentimentTargetStatus.Warming, "status");
    }

    public static void ScheduledSentimentTargetRejectsMovesFromStopped()
    {
        var item = ScheduledSentimentTarget.NewScheduledSentimentTarget("code-371", "name-460");
        item.Status = ScheduledSentimentTargetStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledSentimentTargetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledSentimentTargetStartsInIdle();
        ScheduledSentimentTargetMovesToWarming();
        ScheduledSentimentTargetRejectsMovesFromStopped();
    }
}
