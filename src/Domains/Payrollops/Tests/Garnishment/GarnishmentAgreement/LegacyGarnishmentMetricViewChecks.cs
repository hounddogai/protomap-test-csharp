namespace Acme.Workforce.Payrollops.Tests.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

/// <summary>Checks of garnishment agreement code, run by the project's runner.</summary>
public static class LegacyGarnishmentMetricViewChecks
{
    public static void LegacyGarnishmentMetricViewShowsTheStatusInCapitals()
    {
        var item = LegacyGarnishmentMetric.NewLegacyGarnishmentMetric("code-790", "label-64");
        var view = LegacyGarnishmentMetricView.ToLegacyGarnishmentMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyGarnishmentMetricView.ToLegacyGarnishmentMetricViews(new List<LegacyGarnishmentMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyGarnishmentMetricViewShowsTheStatusInCapitals();
    }
}
