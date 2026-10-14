namespace Acme.Workforce.Engagement.Tests.Feedback.Slot;

using Acme.Workforce.Engagement.Feedback.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class TieredFeedbackPolicyChecks
{
    public static void TieredFeedbackPolicyStartsInRequested()
    {
        var item = TieredFeedbackPolicy.NewTieredFeedbackPolicy("code-563", "title-669");
        Check.Equal(item.Status, TieredFeedbackPolicyStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredFeedbackPolicyMovesToRevoked()
    {
        var item = TieredFeedbackPolicy.NewTieredFeedbackPolicy("code-563", "title-669");
        try
        {
            item.MoveTo(TieredFeedbackPolicyStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredFeedbackPolicyStatus.Revoked, "status");
    }

    public static void TieredFeedbackPolicyRejectsMovesFromExpired()
    {
        var item = TieredFeedbackPolicy.NewTieredFeedbackPolicy("code-563", "title-669");
        item.Status = TieredFeedbackPolicyStatus.Expired;
        Check.Fails(() => item.MoveTo(TieredFeedbackPolicyStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TieredFeedbackPolicyStartsInRequested();
        TieredFeedbackPolicyMovesToRevoked();
        TieredFeedbackPolicyRejectsMovesFromExpired();
    }
}
