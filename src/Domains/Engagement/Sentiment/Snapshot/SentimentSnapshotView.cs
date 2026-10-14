namespace Acme.Workforce.Engagement.Sentiment.Snapshot;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment snapshot.</summary>
public sealed record SentimentSnapshotView(string CodeText, string Title, string Segment, string Maximum, string Summary, string Cost, string NameText, string Label, string StatusLabel)
{
    public static SentimentSnapshotView ToSentimentSnapshotView(SentimentSnapshot item)
    {
        return new SentimentSnapshotView(
            item.Code,
            item.Title,
            item.Segment,
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Summary, 40),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            Text.Truncate(item.Name, 120),
            Text.Truncate(item.Label, 64),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentSnapshotView> ToSentimentSnapshotViews(List<SentimentSnapshot> items)
    {
        var views = new List<SentimentSnapshotView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentSnapshotView(item));
        }
        return views;
    }

    public static double SumSentimentSnapshotView(SentimentSnapshotView view)
    {
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        var parsedCost = double.Parse(view.Cost, CultureInfo.InvariantCulture);
        return parsedMaximum + parsedCost;
    }
}
