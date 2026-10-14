namespace Acme.Workforce.Compensation.Tests.BonusPool.Tier;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BonusPoolTierViewChecks
{
    public static void BonusPoolTierViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolTier.NewBonusPoolTier("code-715", "label-938");
        var view = BonusPoolTierView.ToBonusPoolTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolTierView.ToBonusPoolTierViews(new List<BonusPoolTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolTierViewShowsTheStatusInCapitals();
    }
}
