namespace Acme.Workforce.Compensation.Tests.BonusPool.PoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PoolSchedule;

/// <summary>Checks of pool schedule code, run by the project's runner.</summary>
public static class BonusPoolScoreViewChecks
{
    public static void BonusPoolScoreViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolScore.NewBonusPoolScore("code-228", "title-382");
        var view = BonusPoolScoreView.ToBonusPoolScoreView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolScoreView.ToBonusPoolScoreViews(new List<BonusPoolScore> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolScoreViewShowsTheStatusInCapitals();
    }
}
