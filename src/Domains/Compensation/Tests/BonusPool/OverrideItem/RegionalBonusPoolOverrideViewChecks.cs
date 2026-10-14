namespace Acme.Workforce.Compensation.Tests.BonusPool.OverrideItem;

using Acme.Workforce.Compensation.BonusPool.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RegionalBonusPoolOverrideViewChecks
{
    public static void RegionalBonusPoolOverrideViewShowsTheStatusInCapitals()
    {
        var item = RegionalBonusPoolOverride.NewRegionalBonusPoolOverride("code-971", "label-9");
        var view = RegionalBonusPoolOverrideView.ToRegionalBonusPoolOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RegionalBonusPoolOverrideView.ToRegionalBonusPoolOverrideViews(new List<RegionalBonusPoolOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RegionalBonusPoolOverrideViewShowsTheStatusInCapitals();
    }
}
