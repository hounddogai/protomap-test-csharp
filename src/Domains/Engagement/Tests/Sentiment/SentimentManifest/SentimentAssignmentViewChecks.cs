namespace Acme.Workforce.Engagement.Tests.Sentiment.SentimentManifest;

using Acme.Workforce.Engagement.Sentiment.SentimentManifest;

/// <summary>Checks of sentiment manifest code, run by the project's runner.</summary>
public static class SentimentAssignmentViewChecks
{
    public static void SentimentAssignmentViewShowsTheStatusInCapitals()
    {
        var item = SentimentAssignment.NewSentimentAssignment("code-830", "title-394");
        var view = SentimentAssignmentView.ToSentimentAssignmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SentimentAssignmentView.ToSentimentAssignmentViews(new List<SentimentAssignment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SentimentAssignmentViewShowsTheStatusInCapitals();
    }
}
