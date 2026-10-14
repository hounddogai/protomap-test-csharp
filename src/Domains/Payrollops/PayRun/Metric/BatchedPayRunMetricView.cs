namespace Acme.Workforce.Payrollops.PayRun.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched pay run metric.</summary>
public sealed record BatchedPayRunMetricView(string Code, string TitleText, string LocaleText, string Retries, string Grade, string StatusLabel)
{
    public static BatchedPayRunMetricView ToBatchedPayRunMetricView(BatchedPayRunMetric item)
    {
        return new BatchedPayRunMetricView(
            item.Code,
            Text.Truncate(item.Title, 64),
            item.Locale,
            item.Retries.ToString(CultureInfo.InvariantCulture),
            item.Grade,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedPayRunMetricView> ToBatchedPayRunMetricViews(List<BatchedPayRunMetric> items)
    {
        var views = new List<BatchedPayRunMetricView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedPayRunMetricView(item));
        }
        return views;
    }

    public static double SumBatchedPayRunMetricView(BatchedPayRunMetricView view)
    {
        var parsedRetries = int.Parse(view.Retries, CultureInfo.InvariantCulture);
        return (double)parsedRetries;
    }
}
