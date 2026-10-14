namespace Acme.Workforce.Engagement.Tests.Feedback.Claim;

using Acme.Workforce.Engagement.Feedback.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class PrimaryFeedbackClaimChecks
{
    public static void PrimaryFeedbackClaimStartsInIdle()
    {
        var item = PrimaryFeedbackClaim.NewPrimaryFeedbackClaim("code-806", "title-204");
        Check.Equal(item.Status, PrimaryFeedbackClaimStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryFeedbackClaimMovesToReady()
    {
        var item = PrimaryFeedbackClaim.NewPrimaryFeedbackClaim("code-806", "title-204");
        try
        {
            item.MoveTo(PrimaryFeedbackClaimStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryFeedbackClaimStatus.Ready, "status");
    }

    public static void PrimaryFeedbackClaimRejectsMovesFromStopped()
    {
        var item = PrimaryFeedbackClaim.NewPrimaryFeedbackClaim("code-806", "title-204");
        item.Status = PrimaryFeedbackClaimStatus.Stopped;
        Check.Fails(() => item.MoveTo(PrimaryFeedbackClaimStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PrimaryFeedbackClaimStartsInIdle();
        PrimaryFeedbackClaimMovesToReady();
        PrimaryFeedbackClaimRejectsMovesFromStopped();
    }
}
