namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveRecognitionBatchViewChecks
{
    public static void AdaptiveRecognitionBatchViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("code-203", "label-860");
        var view = AdaptiveRecognitionBatchView.ToAdaptiveRecognitionBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveRecognitionBatchView.ToAdaptiveRecognitionBatchViews(new List<AdaptiveRecognitionBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveRecognitionBatchViewShowsTheStatusInCapitals();
    }
}
