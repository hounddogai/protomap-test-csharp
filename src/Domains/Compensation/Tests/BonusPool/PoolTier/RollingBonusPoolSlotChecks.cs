namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class RollingBonusPoolSlotChecks
{
    public static void RollingBonusPoolSlotStartsInRequested()
    {
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("code-4", "title-550");
        Check.Equal(item.Status, RollingBonusPoolSlotStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingBonusPoolSlotMovesToExpired()
    {
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("code-4", "title-550");
        try
        {
            item.MoveTo(RollingBonusPoolSlotStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingBonusPoolSlotStatus.Expired, "status");
    }

    public static void RollingBonusPoolSlotRejectsMovesFromExpired()
    {
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("code-4", "title-550");
        item.Status = RollingBonusPoolSlotStatus.Expired;
        Check.Fails(() => item.MoveTo(RollingBonusPoolSlotStatus.Requested), "move");
    }

    public static void RunAll()
    {
        RollingBonusPoolSlotStartsInRequested();
        RollingBonusPoolSlotMovesToExpired();
        RollingBonusPoolSlotRejectsMovesFromExpired();
    }
}
