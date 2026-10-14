namespace Acme.Workforce.Engagement.Tests.Recognition.Digest;

using Acme.Workforce.Engagement.Recognition.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RecognitionSegmentViewChecks
{
    public static void RecognitionSegmentViewShowsTheStatusInCapitals()
    {
        var item = RecognitionSegment.NewRecognitionSegment("code-530", "title-102");
        var view = RecognitionSegmentView.ToRecognitionSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RecognitionSegmentView.ToRecognitionSegmentViews(new List<RecognitionSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RecognitionSegmentViewShowsTheStatusInCapitals();
    }
}
