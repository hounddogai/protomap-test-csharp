namespace Acme.Workforce.Assets.Refresh.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a batched refresh metric.</summary>
public sealed record BatchedRefreshMetricView(string Code, string TitleText, string Notes, string CategoryText, string Amount, string Capacity, string Skill, string Limit, string BatchSize, string ContactEmail, string StatusLabel)
{
    public static BatchedRefreshMetricView ToBatchedRefreshMetricView(BatchedRefreshMetric item)
    {
        return new BatchedRefreshMetricView(
            Text.Truncate(item.Code, 120),
            item.Title,
            item.Notes,
            item.Category,
            item.Amount.ToString("F2", CultureInfo.InvariantCulture),
            item.Capacity.ToString(CultureInfo.InvariantCulture),
            item.Skill,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.ContactEmail, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<BatchedRefreshMetricView> ToBatchedRefreshMetricViews(List<BatchedRefreshMetric> items)
    {
        var views = new List<BatchedRefreshMetricView>();
        foreach (var item in items)
        {
            views.Add(ToBatchedRefreshMetricView(item));
        }
        return views;
    }

    public static double SumBatchedRefreshMetricView(BatchedRefreshMetricView view)
    {
        var parsedAmount = double.Parse(view.Amount, CultureInfo.InvariantCulture);
        var parsedCapacity = int.Parse(view.Capacity, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.Limit, CultureInfo.InvariantCulture);
        return parsedAmount + (double)parsedCapacity + (double)parsedLimit;
    }
}
