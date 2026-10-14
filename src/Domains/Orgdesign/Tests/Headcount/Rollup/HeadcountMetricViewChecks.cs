namespace Acme.Workforce.Orgdesign.Tests.Headcount.Rollup;

using Acme.Workforce.Orgdesign.Headcount.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class HeadcountMetricViewChecks
{
    public static void HeadcountMetricViewShowsTheStatusInCapitals()
    {
        var item = HeadcountMetric.NewHeadcountMetric("code-111", "name-767");
        var view = HeadcountMetricView.ToHeadcountMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountMetricView.ToHeadcountMetricViews(new List<HeadcountMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountMetricViewShowsTheStatusInCapitals();
    }
}
