namespace Acme.Workforce.Compensation.Tests.BonusPool.Policy;

using Acme.Workforce.Compensation.BonusPool.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class BonusPoolCycleViewChecks
{
    public static void BonusPoolCycleViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolCycle.NewBonusPoolCycle("code-989", "label-238");
        var view = BonusPoolCycleView.ToBonusPoolCycleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolCycleView.ToBonusPoolCycleViews(new List<BonusPoolCycle> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolCycleViewShowsTheStatusInCapitals();
    }
}
