namespace Acme.Workforce.Payrollops.Tests.RetroPay.Claim;

using Acme.Workforce.Payrollops.RetroPay.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AdaptiveRetroPayMetricViewChecks
{
    public static void AdaptiveRetroPayMetricViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric("code-219", "title-716");
        var view = AdaptiveRetroPayMetricView.ToAdaptiveRetroPayMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveRetroPayMetricView.ToAdaptiveRetroPayMetricViews(new List<AdaptiveRetroPayMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveRetroPayMetricViewShowsTheStatusInCapitals();
    }
}
