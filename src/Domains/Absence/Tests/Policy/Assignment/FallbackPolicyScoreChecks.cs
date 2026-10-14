namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class FallbackPolicyScoreChecks
{
    public static void FallbackPolicyScoreStartsInOpen()
    {
        var item = FallbackPolicyScore.NewFallbackPolicyScore("code-857", "label-836");
        Check.Equal(item.Status, FallbackPolicyScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackPolicyScoreMovesToInReview()
    {
        var item = FallbackPolicyScore.NewFallbackPolicyScore("code-857", "label-836");
        try
        {
            item.MoveTo(FallbackPolicyScoreStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackPolicyScoreStatus.InReview, "status");
    }

    public static void FallbackPolicyScoreRejectsMovesFromArchived()
    {
        var item = FallbackPolicyScore.NewFallbackPolicyScore("code-857", "label-836");
        item.Status = FallbackPolicyScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(FallbackPolicyScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        FallbackPolicyScoreStartsInOpen();
        FallbackPolicyScoreMovesToInReview();
        FallbackPolicyScoreRejectsMovesFromArchived();
    }
}
