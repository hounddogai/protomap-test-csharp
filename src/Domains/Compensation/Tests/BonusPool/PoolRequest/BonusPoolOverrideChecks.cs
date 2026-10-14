namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class BonusPoolOverrideChecks
{
    public static void BonusPoolOverrideStartsInRequested()
    {
        var item = BonusPoolOverride.NewBonusPoolOverride("code-458", "label-172");
        Check.Equal(item.Status, BonusPoolOverrideStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolOverrideMovesToRevoked()
    {
        var item = BonusPoolOverride.NewBonusPoolOverride("code-458", "label-172");
        try
        {
            item.MoveTo(BonusPoolOverrideStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolOverrideStatus.Revoked, "status");
    }

    public static void BonusPoolOverrideRejectsMovesFromExpired()
    {
        var item = BonusPoolOverride.NewBonusPoolOverride("code-458", "label-172");
        item.Status = BonusPoolOverrideStatus.Expired;
        Check.Fails(() => item.MoveTo(BonusPoolOverrideStatus.Requested), "move");
    }

    public static void RunAll()
    {
        BonusPoolOverrideStartsInRequested();
        BonusPoolOverrideMovesToRevoked();
        BonusPoolOverrideRejectsMovesFromExpired();
    }
}
