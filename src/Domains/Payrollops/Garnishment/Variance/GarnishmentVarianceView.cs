namespace Acme.Workforce.Payrollops.Garnishment.Variance;

using System.Globalization;

/// <summary>How screens and exports show a garnishment variance.</summary>
public sealed record GarnishmentVarianceView(string Code, string Label, string UpdatedAt, string Retries, string CostText, string TenureMonths, string StatusLabel)
{
    public static GarnishmentVarianceView ToGarnishmentVarianceView(GarnishmentVariance item)
    {
        return new GarnishmentVarianceView(
            item.Code,
            item.Label,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<GarnishmentVarianceView> ToGarnishmentVarianceViews(List<GarnishmentVariance> items)
    {
        var views = new List<GarnishmentVarianceView>();
        foreach (var item in items)
        {
            views.Add(ToGarnishmentVarianceView(item));
        }
        return views;
    }

    public static double SumGarnishmentVarianceView(GarnishmentVarianceView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + (double)parsedRetries + parsedCost;
    }
}
