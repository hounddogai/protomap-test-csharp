namespace Acme.Workforce.Engagement.Tests.Feedback.Checkpoint;

using Acme.Workforce.Engagement.Feedback.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class FeedbackCheckpointViewChecks
{
    public static void FeedbackCheckpointViewShowsTheStatusInCapitals()
    {
        var item = FeedbackCheckpoint.NewFeedbackCheckpoint("code-610", "name-977");
        var view = FeedbackCheckpointView.ToFeedbackCheckpointView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FeedbackCheckpointView.ToFeedbackCheckpointViews(new List<FeedbackCheckpoint> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FeedbackCheckpointViewShowsTheStatusInCapitals();
    }
}
