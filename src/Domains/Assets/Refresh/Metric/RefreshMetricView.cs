namespace Acme.Workforce.Assets.Refresh.Metric;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a refresh metric.</summary>
public sealed record RefreshMetricView(string Code, string NameText, string Threshold, string Locale, string Unit, string Notes, string DurationMinutesText, string Active, string CategoryText, string BatchSize, string ApprovedText, string StatusLabel)
{
    public static RefreshMetricView ToRefreshMetricView(RefreshMetric item)
    {
        return new RefreshMetricView(
            item.Code,
            item.Name,
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            item.Locale,
            Text.Truncate(item.Unit, 64),
            Text.Truncate(item.Notes, 64),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            $"{item.Active}",
            Text.Truncate(item.Category, 64),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            $"{item.Approved}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RefreshMetricView> ToRefreshMetricViews(List<RefreshMetric> items)
    {
        var views = new List<RefreshMetricView>();
        foreach (var item in items)
        {
            views.Add(ToRefreshMetricView(item));
        }
        return views;
    }

    public static double SumRefreshMetricView(RefreshMetricView view)
    {
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutesText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedThreshold + (double)parsedDurationMinutes + (double)parsedBatchSize;
    }
}
