namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using System.Globalization;

/// <summary>How screens and exports show a batched garnishment metric.</summary>
public sealed record BatchedGarnishmentMetricView(string CodeText, string Name, string ContactEmail, string CreatedAt, string Retries, string Hours, string TenureMonthsText, string StatusLabel)
{
    public static BatchedGarnishmentMetricView ToBatchedGarnishmentMetricView(BatchedGarnishmentMetric item)
    {
        return new BatchedGarnishmentMetricView(
            item.Code,
            item.Name,
            item.ContactEmail,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Hours.ToString("F2", CultureInfo.InvariantCulture),
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedGarnishmentMetricView> ToBatchedGarnishmentMetricViews(List<BatchedGarnishmentMetric> items)
    {
        var views = new List<BatchedGarnishmentMetricView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedGarnishmentMetricView(item));
        }
        return views;
    }

    public static double SumBatchedGarnishmentMetricView(BatchedGarnishmentMetricView view)
    {
        var parsedCreatedAt = int.Parse(view.CreatedAt, CultureInfo.InvariantCulture);
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        var parsedHours = double.Parse(view.Hours, CultureInfo.InvariantCulture);
        return (double)parsedCreatedAt + (double)parsedRetries + parsedHours;
    }
}
