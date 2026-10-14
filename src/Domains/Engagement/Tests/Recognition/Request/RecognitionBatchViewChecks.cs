namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionBatchViewChecks
{
    public static void RecognitionBatchViewShowsTheStatusInCapitals()
    {
        var item = RecognitionBatch.NewRecognitionBatch("code-837", "label-126");
        var view = RecognitionBatchView.ToRecognitionBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionBatchView.ToRecognitionBatchViews(new List<RecognitionBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionBatchViewShowsTheStatusInCapitals();
    }
}
