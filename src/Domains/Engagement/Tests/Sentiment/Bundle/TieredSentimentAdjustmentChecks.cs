namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class TieredSentimentAdjustmentChecks
{
    public static void TieredSentimentAdjustmentStartsInRequested()
    {
        var item = TieredSentimentAdjustment.NewTieredSentimentAdjustment("code-846", "label-685");
        Check.Equal(item.Status, TieredSentimentAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredSentimentAdjustmentMovesToRevoked()
    {
        var item = TieredSentimentAdjustment.NewTieredSentimentAdjustment("code-846", "label-685");
        try
        {
            item.MoveTo(TieredSentimentAdjustmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredSentimentAdjustmentStatus.Revoked, "status");
    }

    public static void TieredSentimentAdjustmentRejectsMovesFromExpired()
    {
        var item = TieredSentimentAdjustment.NewTieredSentimentAdjustment("code-846", "label-685");
        item.Status = TieredSentimentAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredSentimentAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredSentimentAdjustmentStartsInRequested();
        TieredSentimentAdjustmentMovesToRevoked();
        TieredSentimentAdjustmentRejectsMovesFromExpired();
    }
}
