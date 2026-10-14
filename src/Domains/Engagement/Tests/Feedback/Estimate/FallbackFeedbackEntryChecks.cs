namespace Acme.Workforce.Engagement.Tests.Feedback.Estimate;

using Acme.Workforce.Engagement.Feedback.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class FallbackFeedbackEntryChecks
{
    public static void FallbackFeedbackEntryStartsInOpen()
    {
        var item = FallbackFeedbackEntry.NewFallbackFeedbackEntry("code-843", "label-281");
        Check.Equal(item.Status, FallbackFeedbackEntryStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackFeedbackEntryMovesToResolved()
    {
        var item = FallbackFeedbackEntry.NewFallbackFeedbackEntry("code-843", "label-281");
        try
        {
            item.MoveTo(FallbackFeedbackEntryStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackFeedbackEntryStatus.Resolved, "status");
    }

    public static void FallbackFeedbackEntryRejectsMovesFromArchived()
    {
        var item = FallbackFeedbackEntry.NewFallbackFeedbackEntry("code-843", "label-281");
        item.Status = FallbackFeedbackEntryStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackFeedbackEntryStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackFeedbackEntryStartsInOpen();
        FallbackFeedbackEntryMovesToResolved();
        FallbackFeedbackEntryRejectsMovesFromArchived();
    }
}
