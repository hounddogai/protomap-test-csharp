namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class PendingBonusPoolPolicyChecks
{
    public static void PendingBonusPoolPolicyStartsInNew()
    {
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("code-601", "title-236");
        Check.Equal(item.Status, PendingBonusPoolPolicyStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingBonusPoolPolicyMovesToQueued()
    {
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("code-601", "title-236");
        try
        {
            item.MoveTo(PendingBonusPoolPolicyStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingBonusPoolPolicyStatus.Queued, "status");
    }

    public static void PendingBonusPoolPolicyRejectsMovesFromFailed()
    {
        var item = PendingBonusPoolPolicy.NewPendingBonusPoolPolicy("code-601", "title-236");
        item.Status = PendingBonusPoolPolicyStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingBonusPoolPolicyStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingBonusPoolPolicyStartsInNew();
        PendingBonusPoolPolicyMovesToQueued();
        PendingBonusPoolPolicyRejectsMovesFromFailed();
    }
}
