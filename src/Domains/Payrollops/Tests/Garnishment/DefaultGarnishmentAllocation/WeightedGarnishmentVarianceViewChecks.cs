namespace Acme.Workforce.Payrollops.Tests.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

/// <summary>Checks of default garnishment allocation code, run by the project's runner.</summary>
public static class WeightedGarnishmentVarianceViewChecks
{
    public static void WeightedGarnishmentVarianceViewShowsTheStatusInCapitals()
    {
        var item = WeightedGarnishmentVariance.NewWeightedGarnishmentVariance("code-379", "title-522");
        var view = WeightedGarnishmentVarianceView.ToWeightedGarnishmentVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedGarnishmentVarianceView.ToWeightedGarnishmentVarianceViews(new List<WeightedGarnishmentVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedGarnishmentVarianceViewShowsTheStatusInCapitals();
    }
}
