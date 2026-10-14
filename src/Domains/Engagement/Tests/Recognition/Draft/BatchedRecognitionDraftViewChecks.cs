namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class BatchedRecognitionDraftViewChecks
{
    public static void BatchedRecognitionDraftViewShowsTheStatusInCapitals()
    {
        var item = BatchedRecognitionDraft.NewBatchedRecognitionDraft("code-802", "name-202");
        var view = BatchedRecognitionDraftView.ToBatchedRecognitionDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedRecognitionDraftView.ToBatchedRecognitionDraftViews(new List<BatchedRecognitionDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedRecognitionDraftViewShowsTheStatusInCapitals();
    }
}
