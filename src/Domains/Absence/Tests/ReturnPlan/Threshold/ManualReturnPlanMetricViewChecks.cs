namespace Acme.Workforce.Absence.Tests.ReturnPlan.Threshold;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class ManualReturnPlanMetricViewChecks
{
    public static void ManualReturnPlanMetricViewShowsTheStatusInCapitals()
    {
        var item = ManualReturnPlanMetric.NewManualReturnPlanMetric("code-40", "name-521");
        var view = ManualReturnPlanMetricView.ToManualReturnPlanMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualReturnPlanMetricView.ToManualReturnPlanMetricViews(new List<ManualReturnPlanMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualReturnPlanMetricViewShowsTheStatusInCapitals();
    }
}
