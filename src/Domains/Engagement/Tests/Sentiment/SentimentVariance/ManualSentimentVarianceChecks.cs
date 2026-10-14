namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentVariance;

using Acme.Workforce.Engagement.Sentiment.SentimentVariance;

/// <summary>Checks of sentiment variance code, run by the project's runner.</summary>
public static class ManualSentimentVarianceChecks
{
    public static void ManualSentimentVarianceStartsInPlanned()
    {
        var item = ManualSentimentVariance.NewManualSentimentVariance("code-757", "label-858");
        Check.Equal(item.Status, ManualSentimentVarianceStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualSentimentVarianceMovesToScheduled()
    {
        var item = ManualSentimentVariance.NewManualSentimentVariance("code-757", "label-858");
        try
        {
            item.MoveTo(ManualSentimentVarianceStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualSentimentVarianceStatus.Scheduled, "status");
    }

    public static void ManualSentimentVarianceRejectsMovesFromCancelled()
    {
        var item = ManualSentimentVariance.NewManualSentimentVariance("code-757", "label-858");
        item.Status = ManualSentimentVarianceStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualSentimentVarianceStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualSentimentVarianceStartsInPlanned();
        ManualSentimentVarianceMovesToScheduled();
        ManualSentimentVarianceRejectsMovesFromCancelled();
    }
}
