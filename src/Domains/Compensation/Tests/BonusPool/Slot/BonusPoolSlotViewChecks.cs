namespace Acme.Workforce.Compensation.Tests.BonusPool.Slot;

using Acme.Workforce.Compensation.BonusPool.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class BonusPoolSlotViewChecks
{
    public static void BonusPoolSlotViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolSlot.NewBonusPoolSlot("code-881", "name-222");
        var view = BonusPoolSlotView.ToBonusPoolSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolSlotView.ToBonusPoolSlotViews(new List<BonusPoolSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolSlotViewShowsTheStatusInCapitals();
    }
}
