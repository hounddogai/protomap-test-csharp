namespace Acme.Workforce.Engagement.Sentiment.Baseline;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a weighted sentiment manifest.</summary>
public sealed record WeightedSentimentManifestView(string CodeText, string Title, string VersionText, string Department, string LimitText, string Visible, string Margin, string Phone, string Timezone, string StatusLabel)
{
    public static WeightedSentimentManifestView ToWeightedSentimentManifestView(WeightedSentimentManifest item)
    {
        return new WeightedSentimentManifestView(
            Text.Truncate(item.Code, 64),
            Text.Truncate(item.Title, 64),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Department,
            item.Limit.ToString(CultureInfo.InvariantCulture),
            $"{item.Visible}",
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Phone,
            item.Timezone,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedSentimentManifestView> ToWeightedSentimentManifestViews(List<WeightedSentimentManifest> items)
    {
        var views = new List<WeightedSentimentManifestView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedSentimentManifestView(item));
        }
        return views;
    }

    public static double SumWeightedSentimentManifestView(WeightedSentimentManifestView view)
    {
        var parsedVersion = int.Parse(view.VersionText, CultureInfo.InvariantCulture);
        var parsedLimit = int.Parse(view.LimitText, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedLimit + parsedMargin;
    }
}
