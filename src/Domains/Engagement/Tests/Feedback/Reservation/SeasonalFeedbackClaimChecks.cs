namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SeasonalFeedbackClaimChecks
{
    public static void SeasonalFeedbackClaimStartsInProposed()
    {
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("code-619", "title-251");
        Check.Equal(item.Status, SeasonalFeedbackClaimStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalFeedbackClaimMovesToRejected()
    {
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("code-619", "title-251");
        try
        {
            item.MoveTo(SeasonalFeedbackClaimStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalFeedbackClaimStatus.Rejected, "status");
    }

    public static void SeasonalFeedbackClaimRejectsMovesFromWithdrawn()
    {
        var item = SeasonalFeedbackClaim.NewSeasonalFeedbackClaim("code-619", "title-251");
        item.Status = SeasonalFeedbackClaimStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalFeedbackClaimStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalFeedbackClaimStartsInProposed();
        SeasonalFeedbackClaimMovesToRejected();
        SeasonalFeedbackClaimRejectsMovesFromWithdrawn();
    }
}
