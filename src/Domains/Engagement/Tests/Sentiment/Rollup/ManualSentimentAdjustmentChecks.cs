namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualSentimentAdjustmentChecks
{
    public static void ManualSentimentAdjustmentStartsInCreated()
    {
        var item = ManualSentimentAdjustment.NewManualSentimentAdjustment("code-908", "name-326");
        Check.Equal(item.Status, ManualSentimentAdjustmentStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualSentimentAdjustmentMovesToPublished()
    {
        var item = ManualSentimentAdjustment.NewManualSentimentAdjustment("code-908", "name-326");
        try
        {
            item.MoveTo(ManualSentimentAdjustmentStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualSentimentAdjustmentStatus.Published, "status");
    }

    public static void ManualSentimentAdjustmentRejectsMovesFromRetired()
    {
        var item = ManualSentimentAdjustment.NewManualSentimentAdjustment("code-908", "name-326");
        item.Status = ManualSentimentAdjustmentStatus.Retired;
        Check.Fails(() => item.MoveTo(ManualSentimentAdjustmentStatus.Created), "move");
    }

    public static void RunAll()
    {
        ManualSentimentAdjustmentStartsInCreated();
        ManualSentimentAdjustmentMovesToPublished();
        ManualSentimentAdjustmentRejectsMovesFromRetired();
    }
}
