namespace Acme.Workforce.Engagement.Sentiment.Manifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a sentiment manifest.</summary>
public sealed record SentimentManifestView(string Code, string Label, string CategoryText, string ExternalRefText, string Segment, string CostText, string BatchSizeText, string CostCenter, string StatusLabel)
{
    public static SentimentManifestView ToSentimentManifestView(SentimentManifest item)
    {
        return new SentimentManifestView(
            Text.Truncate(item.Code, 64),
            item.Label,
            item.Category,
            Text.Truncate(item.ExternalRef, 64),
            item.Segment,
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.CostCenter,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<SentimentManifestView> ToSentimentManifestViews(List<SentimentManifest> items)
    {
        var views = new List<SentimentManifestView>();
        foreach (var item in items)
        {
            views.Add(ToSentimentManifestView(item));
        }
        return views;
    }

    public static double SumSentimentManifestView(SentimentManifestView view)
    {
        var parsedCost = double.Parse(view.CostText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSizeText, CultureInfo.InvariantCulture);
        return parsedCost + (double)parsedBatchSize;
    }
}
