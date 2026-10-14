namespace Acme.Workforce.Payrollops.Tests.Deduction.Agreement;

using Acme.Workforce.Payrollops.Deduction.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class DeductionMetricViewChecks
{
    public static void DeductionMetricViewShowsTheStatusInCapitals()
    {
        var item = DeductionMetric.NewDeductionMetric("code-994", "name-342");
        var view = DeductionMetricView.ToDeductionMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DeductionMetricView.ToDeductionMetricViews(new List<DeductionMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DeductionMetricViewShowsTheStatusInCapitals();
    }
}
