namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class BonusPoolOverrideViewChecks
{
    public static void BonusPoolOverrideViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolOverride.NewBonusPoolOverride("code-458", "label-172");
        var view = BonusPoolOverrideView.ToBonusPoolOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolOverrideView.ToBonusPoolOverrideViews(new List<BonusPoolOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolOverrideViewShowsTheStatusInCapitals();
    }
}
