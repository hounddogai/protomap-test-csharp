namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a capped sentiment manifest.</summary>
public sealed record CappedSentimentManifestView(string Code, string Title, string Currency, string Channel, string CeilingText, string FloorValueText, string BatchSize, string ReviewerText, string StatusLabel)
{
    public static CappedSentimentManifestView ToCappedSentimentManifestView(CappedSentimentManifest item)
    {
        return new CappedSentimentManifestView(
            item.Code,
            Text.Truncate(item.Title, 120),
            item.Currency,
            Text.Truncate(item.Channel, 64),
            item.Ceiling.ToString("F2", CultureInfo.InvariantCulture),
            item.FloorValue.ToString("F2", CultureInfo.InvariantCulture),
            item.BatchSize.ToString(CultureInfo.InvariantCulture),
            item.Reviewer,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<CappedSentimentManifestView> ToCappedSentimentManifestViews(List<CappedSentimentManifest> items)
    {
        var views = new List<CappedSentimentManifestView>();
        foreach (var item in items)
        {
            views.Add(ToCappedSentimentManifestView(item));
        }
        return views;
    }

    public static double SumCappedSentimentManifestView(CappedSentimentManifestView view)
    {
        var parsedCeiling = double.Parse(view.CeilingText, CultureInfo.InvariantCulture);
        var parsedFloorValue = double.Parse(view.FloorValueText, CultureInfo.InvariantCulture);
        var parsedBatchSize = int.Parse(view.BatchSize, CultureInfo.InvariantCulture);
        return parsedCeiling + parsedFloorValue + (double)parsedBatchSize;
    }
}
