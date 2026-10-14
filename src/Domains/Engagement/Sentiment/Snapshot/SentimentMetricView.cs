namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment metric.</summary>
public sealed record SentimentMetricView(string Code, string NameText, string UpdatedAt, string MaximumText, string Quantity, string Attempts, string Location, string CreatedAt, string StatusLabel)
{
    public static SentimentMetricView ToSentimentMetricView(SentimentMetric item)
    {
        return new SentimentMetricView(
            Text.Truncate(item.Code, 40),
            item.Name,
            item.UpdatedAt.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Attempts.ToString(CultureInfo.InvariantCulture),
            item.Location,
            item.CreatedAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentMetricView> ToSentimentMetricViews(List<SentimentMetric> items)
    {
        var views = new List<SentimentMetricView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentMetricView(item));
        }
        return views;
    }

    public static double SumSentimentMetricView(SentimentMetricView view)
    {
        var parsedUpdatedAt = int.Parse(view.UpdatedAt, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.MaximumText, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedUpdatedAt + parsedMaximum + (double)parsedQuantity;
    }
}
