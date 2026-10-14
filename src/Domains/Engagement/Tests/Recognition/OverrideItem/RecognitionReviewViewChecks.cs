namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionReviewViewChecks
{
    public static void RecognitionReviewViewShowsTheStatusInCapitals()
    {
        var item = RecognitionReview.NewRecognitionReview("code-230", "title-310");
        var view = RecognitionReviewView.ToRecognitionReviewView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionReviewView.ToRecognitionReviewViews(new List<RecognitionReview> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionReviewViewShowsTheStatusInCapitals();
    }
}
