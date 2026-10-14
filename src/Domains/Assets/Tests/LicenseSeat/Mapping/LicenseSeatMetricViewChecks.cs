namespace Acme.Workforce.Assets.Tests.LicenseSeat.Mapping;

using Acme.Workforce.Assets.LicenseSeat.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LicenseSeatMetricViewChecks
{
    public static void LicenseSeatMetricViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatMetric.NewLicenseSeatMetric("code-417", "label-53");
        var view = LicenseSeatMetricView.ToLicenseSeatMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatMetricView.ToLicenseSeatMetricViews(new List<LicenseSeatMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatMetricViewShowsTheStatusInCapitals();
    }
}
