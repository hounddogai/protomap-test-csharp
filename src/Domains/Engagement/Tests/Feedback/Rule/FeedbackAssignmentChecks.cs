namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FeedbackAssignmentChecks
{
    public static void FeedbackAssignmentStartsInDraft()
    {
        var item = FeedbackAssignment.NewFeedbackAssignment("code-743", "name-690");
        Check.Equal(item.Status, FeedbackAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackAssignmentMovesToActive()
    {
        var item = FeedbackAssignment.NewFeedbackAssignment("code-743", "name-690");
        try
        {
            item.MoveTo(FeedbackAssignmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackAssignmentStatus.Active, "status");
    }

    public static void FeedbackAssignmentRejectsMovesFromClosed()
    {
        var item = FeedbackAssignment.NewFeedbackAssignment("code-743", "name-690");
        item.Status = FeedbackAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(FeedbackAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        FeedbackAssignmentStartsInDraft();
        FeedbackAssignmentMovesToActive();
        FeedbackAssignmentRejectsMovesFromClosed();
    }
}
