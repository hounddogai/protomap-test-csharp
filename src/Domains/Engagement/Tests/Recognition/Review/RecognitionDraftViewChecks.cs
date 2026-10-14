namespace Acme.Workforce.Engagement.Tests.Recognition.Review;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RecognitionDraftViewChecks
{
    public static void RecognitionDraftViewShowsTheStatusInCapitals()
    {
        var item = RecognitionDraft.NewRecognitionDraft("code-551", "name-745");
        var view = RecognitionDraftView.ToRecognitionDraftView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionDraftView.ToRecognitionDraftViews(new List<RecognitionDraft> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionDraftViewShowsTheStatusInCapitals();
    }
}
