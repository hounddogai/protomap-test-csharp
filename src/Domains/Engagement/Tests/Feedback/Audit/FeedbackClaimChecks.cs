namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class FeedbackClaimChecks
{
    public static void FeedbackClaimStartsInRequested()
    {
        var item = FeedbackClaim.NewFeedbackClaim("code-636", "name-516");
        Check.Equal(item.Status, FeedbackClaimStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackClaimMovesToGranted()
    {
        var item = FeedbackClaim.NewFeedbackClaim("code-636", "name-516");
        try
        {
            item.MoveTo(FeedbackClaimStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackClaimStatus.Granted, "status");
    }

    public static void FeedbackClaimRejectsMovesFromExpired()
    {
        var item = FeedbackClaim.NewFeedbackClaim("code-636", "name-516");
        item.Status = FeedbackClaimStatus.Expired;
        Check.Fails(() => item.MoveTo(FeedbackClaimStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FeedbackClaimStartsInRequested();
        FeedbackClaimMovesToGranted();
        FeedbackClaimRejectsMovesFromExpired();
    }
}
