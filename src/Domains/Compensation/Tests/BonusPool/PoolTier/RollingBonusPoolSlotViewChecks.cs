namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolTier;

using Acme.Workforce.Compensation.BonusPool.PoolTier;

/// <summary>Checks of pool tier code, run by the project's runner.</summary>
public static class RollingBonusPoolSlotViewChecks
{
    public static void RollingBonusPoolSlotViewShowsTheStatusInCapitals()
    {
        var item = RollingBonusPoolSlot.NewRollingBonusPoolSlot("code-4", "title-550");
        var view = RollingBonusPoolSlotView.ToRollingBonusPoolSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingBonusPoolSlotView.ToRollingBonusPoolSlotViews(new List<RollingBonusPoolSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingBonusPoolSlotViewShowsTheStatusInCapitals();
    }
}
