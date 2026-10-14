namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BonusPoolRollupViewChecks
{
    public static void BonusPoolRollupViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolRollup.NewBonusPoolRollup("code-533", "name-818");
        var view = BonusPoolRollupView.ToBonusPoolRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolRollupView.ToBonusPoolRollupViews(new List<BonusPoolRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolRollupViewShowsTheStatusInCapitals();
    }
}
