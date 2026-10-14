namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopMetricViewChecks
{
    public static void LaptopMetricViewShowsTheStatusInCapitals()
    {
        var item = LaptopMetric.NewLaptopMetric("code-46", "label-739");
        var view = LaptopMetricView.ToLaptopMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LaptopMetricView.ToLaptopMetricViews(new List<LaptopMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LaptopMetricViewShowsTheStatusInCapitals();
    }
}
