namespace Acme.Workforce.Assets.Tests.Laptop.Metric;

using Acme.Workforce.Assets.Laptop.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class LenientLaptopMetricViewChecks
{
    public static void LenientLaptopMetricViewShowsTheStatusInCapitals()
    {
        var item = LenientLaptopMetric.NewLenientLaptopMetric("code-880", "name-518");
        var view = LenientLaptopMetricView.ToLenientLaptopMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientLaptopMetricView.ToLenientLaptopMetricViews(new List<LenientLaptopMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientLaptopMetricViewShowsTheStatusInCapitals();
    }
}
