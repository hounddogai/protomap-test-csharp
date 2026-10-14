namespace Acme.Workforce.Compensation.Tests.BonusPool.Batch;

using Acme.Workforce.Compensation.BonusPool.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class BonusPoolMilestoneViewChecks
{
    public static void BonusPoolMilestoneViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolMilestone.NewBonusPoolMilestone("code-658", "title-294");
        var view = BonusPoolMilestoneView.ToBonusPoolMilestoneView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolMilestoneView.ToBonusPoolMilestoneViews(new List<BonusPoolMilestone> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolMilestoneViewShowsTheStatusInCapitals();
    }
}
