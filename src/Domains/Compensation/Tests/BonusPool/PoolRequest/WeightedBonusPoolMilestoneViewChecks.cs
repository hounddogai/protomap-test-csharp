namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolRequest;

using Acme.Workforce.Compensation.BonusPool.PoolRequest;

/// <summary>Checks of pool request code, run by the project's runner.</summary>
public static class WeightedBonusPoolMilestoneViewChecks
{
    public static void WeightedBonusPoolMilestoneViewShowsTheStatusInCapitals()
    {
        var item = WeightedBonusPoolMilestone.NewWeightedBonusPoolMilestone("code-24", "label-525");
        var view = WeightedBonusPoolMilestoneView.ToWeightedBonusPoolMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedBonusPoolMilestoneView.ToWeightedBonusPoolMilestoneViews(new List<WeightedBonusPoolMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedBonusPoolMilestoneViewShowsTheStatusInCapitals();
    }
}
