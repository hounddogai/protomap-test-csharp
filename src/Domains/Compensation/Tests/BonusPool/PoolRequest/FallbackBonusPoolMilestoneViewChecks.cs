namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class FallbackBonusPoolMilestoneViewChecks
{
    public static void FallbackBonusPoolMilestoneViewShowsTheStatusInCapitals()
    {
        var item = FallbackBonusPoolMilestone.NewFallbackBonusPoolMilestone("code-231", "name-436");
        var view = FallbackBonusPoolMilestoneView.ToFallbackBonusPoolMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackBonusPoolMilestoneView.ToFallbackBonusPoolMilestoneViews(new List<FallbackBonusPoolMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackBonusPoolMilestoneViewShowsTheStatusInCapitals();
    }
}
