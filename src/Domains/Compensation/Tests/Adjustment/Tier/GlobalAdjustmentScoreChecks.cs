namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GlobalAdjustmentScoreChecks
{
    public static void GlobalAdjustmentScoreStartsInRequested()
    {
        var item = GlobalAdjustmentScore.NewGlobalAdjustmentScore("code-717", "title-84");
        Check.Equal(item.Status, GlobalAdjustmentScoreStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAdjustmentScoreMovesToRevoked()
    {
        var item = GlobalAdjustmentScore.NewGlobalAdjustmentScore("code-717", "title-84");
        try
        {
            item.MoveTo(GlobalAdjustmentScoreStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAdjustmentScoreStatus.Revoked, "status");
    }

    public static void GlobalAdjustmentScoreRejectsMovesFromExpired()
    {
        var item = GlobalAdjustmentScore.NewGlobalAdjustmentScore("code-717", "title-84");
        item.Status = GlobalAdjustmentScoreStatus.Expired;
        Check.Fails(() => item.MoveTo(GlobalAdjustmentScoreStatus.Requested), "move");
    }

    public static void RunAll()
    {
        GlobalAdjustmentScoreStartsInRequested();
        GlobalAdjustmentScoreMovesToRevoked();
        GlobalAdjustmentScoreRejectsMovesFromExpired();
    }
}
