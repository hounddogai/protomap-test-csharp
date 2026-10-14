namespace Acme.Workforce.Payrollops.Tests.Garnishment.Ledger;

using Acme.Workforce.Payrollops.Garnishment.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GarnishmentEstimateViewChecks
{
    public static void GarnishmentEstimateViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentEstimate.NewGarnishmentEstimate("code-805", "name-566");
        var view = GarnishmentEstimateView.ToGarnishmentEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentEstimateView.ToGarnishmentEstimateViews(new List<GarnishmentEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentEstimateViewShowsTheStatusInCapitals();
    }
}
