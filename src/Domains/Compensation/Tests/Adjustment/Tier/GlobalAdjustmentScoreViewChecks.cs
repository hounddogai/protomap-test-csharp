namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class GlobalAdjustmentScoreViewChecks
{
    public static void GlobalAdjustmentScoreViewShowsTheStatusInCapitals()
    {
        var item = GlobalAdjustmentScore.NewGlobalAdjustmentScore("code-717", "title-84");
        var view = GlobalAdjustmentScoreView.ToGlobalAdjustmentScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalAdjustmentScoreView.ToGlobalAdjustmentScoreViews(new List<GlobalAdjustmentScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalAdjustmentScoreViewShowsTheStatusInCapitals();
    }
}
