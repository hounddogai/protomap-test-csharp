namespace Acme.Workforce.Absence.Tests.Accrual.Rollup;

using Acme.Workforce.Absence.Accrual.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class AccrualMetricViewChecks
{
    public static void AccrualMetricViewShowsTheStatusInCapitals()
    {
        var item = AccrualMetric.NewAccrualMetric("code-885", "label-3");
        var view = AccrualMetricView.ToAccrualMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AccrualMetricView.ToAccrualMetricViews(new List<AccrualMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AccrualMetricViewShowsTheStatusInCapitals();
    }
}
