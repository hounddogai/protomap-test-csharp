namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LenientBonusPoolSlotViewChecks
{
    public static void LenientBonusPoolSlotViewShowsTheStatusInCapitals()
    {
        var item = LenientBonusPoolSlot.NewLenientBonusPoolSlot("code-203", "title-221");
        var view = LenientBonusPoolSlotView.ToLenientBonusPoolSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientBonusPoolSlotView.ToLenientBonusPoolSlotViews(new List<LenientBonusPoolSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientBonusPoolSlotViewShowsTheStatusInCapitals();
    }
}
