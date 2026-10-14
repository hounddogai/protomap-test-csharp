namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackWindowChecks
{
    public static void FeedbackWindowStartsInOpen()
    {
        var item = FeedbackWindow.NewFeedbackWindow("code-69", "title-350");
        Check.Equal(item.Status, FeedbackWindowStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackWindowMovesToResolved()
    {
        var item = FeedbackWindow.NewFeedbackWindow("code-69", "title-350");
        try
        {
            item.MoveTo(FeedbackWindowStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackWindowStatus.Resolved, "status");
    }

    public static void FeedbackWindowRejectsMovesFromArchived()
    {
        var item = FeedbackWindow.NewFeedbackWindow("code-69", "title-350");
        item.Status = FeedbackWindowStatus.Archived;
        Check.Fails(() => item.MoveTo(FeedbackWindowStatus.Open), "move");
    }

    public static void RunAll()
    {
        FeedbackWindowStartsInOpen();
        FeedbackWindowMovesToResolved();
        FeedbackWindowRejectsMovesFromArchived();
    }
}
