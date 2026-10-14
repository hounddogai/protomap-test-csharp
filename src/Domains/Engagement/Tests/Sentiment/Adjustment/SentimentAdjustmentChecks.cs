namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentAdjustmentChecks
{
    public static void SentimentAdjustmentStartsInRequested()
    {
        var item = SentimentAdjustment.NewSentimentAdjustment("code-337", "title-94");
        Check.Equal(item.Status, SentimentAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SentimentAdjustmentMovesToRevoked()
    {
        var item = SentimentAdjustment.NewSentimentAdjustment("code-337", "title-94");
        try
        {
            item.MoveTo(SentimentAdjustmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SentimentAdjustmentStatus.Revoked, "status");
    }

    public static void SentimentAdjustmentRejectsMovesFromExpired()
    {
        var item = SentimentAdjustment.NewSentimentAdjustment("code-337", "title-94");
        item.Status = SentimentAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(SentimentAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SentimentAdjustmentStartsInRequested();
        SentimentAdjustmentMovesToRevoked();
        SentimentAdjustmentRejectsMovesFromExpired();
    }
}
