namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentRollupViewChecks
{
    public static void GarnishmentRollupViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentRollup.NewGarnishmentRollup("code-256", "label-702");
        var view = GarnishmentRollupView.ToGarnishmentRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentRollupView.ToGarnishmentRollupViews(new List<GarnishmentRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentRollupViewShowsTheStatusInCapitals();
    }
}
