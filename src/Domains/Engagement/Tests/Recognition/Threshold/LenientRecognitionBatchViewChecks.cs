namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientRecognitionBatchViewChecks
{
    public static void LenientRecognitionBatchViewShowsTheStatusInCapitals()
    {
        var item = LenientRecognitionBatch.NewLenientRecognitionBatch("code-578", "label-370");
        var view = LenientRecognitionBatchView.ToLenientRecognitionBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientRecognitionBatchView.ToLenientRecognitionBatchViews(new List<LenientRecognitionBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientRecognitionBatchViewShowsTheStatusInCapitals();
    }
}
