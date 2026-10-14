namespace Acme.Workforce.Engagement.Recognition.Window;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a scheduled recognition metric.</summary>
public sealed record ScheduledRecognitionMetricView(string Code, string Name, string BatchSize, string Threshold, string Active, string StatusLabel)
{
    public static ScheduledRecognitionMetricView ToScheduledRecognitionMetricView(ScheduledRecognitionMetric item)
    {
        return new ScheduledRecognitionMetricView(
            Text.Truncate(item.Code, 40),
            Text.Truncate(item.Name, 80),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Threshold.ToString("F2", CultureInfo.InvariantCulture),
            $"{item.Active}",
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<ScheduledRecognitionMetricView> ToScheduledRecognitionMetricViews(List<ScheduledRecognitionMetric> items)
    {
        var views = new List<ScheduledRecognitionMetricView>();
        foreach (var item in items)
        {
            views.Add(ToScheduledRecognitionMetricView(item));
        }
        return views;
    }

    public static double SumScheduledRecognitionMetricView(ScheduledRecognitionMetricView view)
    {
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        var parsedThreshold = double.Parse(view.Threshold, CultureInfo.InvariantCulture);
        return (double)parsedBatchSize + parsedThreshold;
    }
}
