namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackManifestChecks
{
    public static void FeedbackManifestStartsInPlanned()
    {
        var item = FeedbackManifest.NewFeedbackManifest("code-111", "name-730");
        Check.Equal(item.Status, FeedbackManifestStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackManifestMovesToCompleted()
    {
        var item = FeedbackManifest.NewFeedbackManifest("code-111", "name-730");
        try
        {
            item.MoveTo(FeedbackManifestStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackManifestStatus.Completed, "status");
    }

    public static void FeedbackManifestRejectsMovesFromCancelled()
    {
        var item = FeedbackManifest.NewFeedbackManifest("code-111", "name-730");
        item.Status = FeedbackManifestStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FeedbackManifestStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FeedbackManifestStartsInPlanned();
        FeedbackManifestMovesToCompleted();
        FeedbackManifestRejectsMovesFromCancelled();
    }
}
