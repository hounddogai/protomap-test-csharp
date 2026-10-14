namespace Acme.Workforce.Engagement.Tests.Sentiment.Threshold;

using Acme.Workforce.Engagement.Sentiment.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientSentimentThresholdViewChecks
{
    public static void LenientSentimentThresholdViewShowsTheStatusInCapitals()
    {
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("code-100", "label-929");
        var view = LenientSentimentThresholdView.ToLenientSentimentThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientSentimentThresholdView.ToLenientSentimentThresholdViews(new List<LenientSentimentThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientSentimentThresholdViewShowsTheStatusInCapitals();
    }
}
