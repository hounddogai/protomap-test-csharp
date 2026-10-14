namespace Acme.Workforce.Compensation.Tests.BonusPool.Rollup;

using Acme.Workforce.Compensation.BonusPool.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BonusPoolPlanViewChecks
{
    public static void BonusPoolPlanViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolPlan.NewBonusPoolPlan("code-46", "name-929");
        var view = BonusPoolPlanView.ToBonusPoolPlanView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolPlanView.ToBonusPoolPlanViews(new List<BonusPoolPlan> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolPlanViewShowsTheStatusInCapitals();
    }
}
