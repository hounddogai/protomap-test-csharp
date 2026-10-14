namespace Acme.Workforce.Compensation.Tests.BonusPool.Allocation;

using Acme.Workforce.Compensation.BonusPool.Allocation;

/// <summary>Checks of allocation code, run by the project's runner.</summary>
public static class LenientBonusPoolPolicyChecks
{
    public static void LenientBonusPoolPolicyStartsInOpen()
    {
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("code-372", "name-349");
        Check.Equal(item.Status, LenientBonusPoolPolicyStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBonusPoolPolicyMovesToInReview()
    {
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("code-372", "name-349");
        try
        {
            item.MoveTo(LenientBonusPoolPolicyStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBonusPoolPolicyStatus.InReview, "status");
    }

    public static void LenientBonusPoolPolicyRejectsMovesFromArchived()
    {
        var item = LenientBonusPoolPolicy.NewLenientBonusPoolPolicy("code-372", "name-349");
        item.Status = LenientBonusPoolPolicyStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientBonusPoolPolicyStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientBonusPoolPolicyStartsInOpen();
        LenientBonusPoolPolicyMovesToInReview();
        LenientBonusPoolPolicyRejectsMovesFromArchived();
    }
}
