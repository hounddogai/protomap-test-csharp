namespace Acme.Workforce.Absence.Tests.Accrual.Checkpoint;

using Acme.Workforce.Absence.Accrual.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AdaptiveAccrualMetricViewChecks
{
    public static void AdaptiveAccrualMetricViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveAccrualMetric.NewAdaptiveAccrualMetric("code-604", "title-11");
        var view = AdaptiveAccrualMetricView.ToAdaptiveAccrualMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveAccrualMetricView.ToAdaptiveAccrualMetricViews(new List<AdaptiveAccrualMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveAccrualMetricViewShowsTheStatusInCapitals();
    }
}
