namespace Acme.Workforce.Compensation.Tests.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Checks of primary pool schedule code, run by the project's runner.</summary>
public static class BonusPoolMetricViewChecks
{
    public static void BonusPoolMetricViewShowsTheStatusInCapitals()
    {
        var item = BonusPoolMetric.NewBonusPoolMetric("code-837", "label-775");
        var view = BonusPoolMetricView.ToBonusPoolMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BonusPoolMetricView.ToBonusPoolMetricViews(new List<BonusPoolMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BonusPoolMetricViewShowsTheStatusInCapitals();
    }
}
