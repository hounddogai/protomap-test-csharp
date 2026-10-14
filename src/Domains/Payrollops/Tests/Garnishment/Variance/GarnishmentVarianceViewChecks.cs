namespace Acme.Workforce.Payrollops.Tests.Garnishment.Variance;

using Acme.Workforce.Payrollops.Garnishment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GarnishmentVarianceViewChecks
{
    public static void GarnishmentVarianceViewShowsTheStatusInCapitals()
    {
        var item = GarnishmentVariance.NewGarnishmentVariance("code-858", "label-818");
        var view = GarnishmentVarianceView.ToGarnishmentVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GarnishmentVarianceView.ToGarnishmentVarianceViews(new List<GarnishmentVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GarnishmentVarianceViewShowsTheStatusInCapitals();
    }
}
