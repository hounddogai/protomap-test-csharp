namespace Acme.Workforce.Engagement.Tests.Sentiment.Rollup;

using Acme.Workforce.Engagement.Sentiment.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class ManualSentimentAdjustmentViewChecks
{
    public static void ManualSentimentAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = ManualSentimentAdjustment.NewManualSentimentAdjustment("code-908", "name-326");
        var view = ManualSentimentAdjustmentView.ToManualSentimentAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualSentimentAdjustmentView.ToManualSentimentAdjustmentViews(new List<ManualSentimentAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualSentimentAdjustmentViewShowsTheStatusInCapitals();
    }
}
