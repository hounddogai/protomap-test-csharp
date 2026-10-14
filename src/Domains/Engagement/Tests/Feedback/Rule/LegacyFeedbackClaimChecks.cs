namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class LegacyFeedbackClaimChecks
{
    public static void LegacyFeedbackClaimStartsInIdle()
    {
        var item = LegacyFeedbackClaim.NewLegacyFeedbackClaim("code-454", "label-764");
        Check.Equal(item.Status, LegacyFeedbackClaimStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyFeedbackClaimMovesToWarming()
    {
        var item = LegacyFeedbackClaim.NewLegacyFeedbackClaim("code-454", "label-764");
        try
        {
            item.MoveTo(LegacyFeedbackClaimStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyFeedbackClaimStatus.Warming, "status");
    }

    public static void LegacyFeedbackClaimRejectsMovesFromStopped()
    {
        var item = LegacyFeedbackClaim.NewLegacyFeedbackClaim("code-454", "label-764");
        item.Status = LegacyFeedbackClaimStatus.Stopped;
        Check.Fails(() => item.MoveTo(LegacyFeedbackClaimStatus.Idle), "move");
    }

    public static void RunAll()
    {
        LegacyFeedbackClaimStartsInIdle();
        LegacyFeedbackClaimMovesToWarming();
        LegacyFeedbackClaimRejectsMovesFromStopped();
    }
}
