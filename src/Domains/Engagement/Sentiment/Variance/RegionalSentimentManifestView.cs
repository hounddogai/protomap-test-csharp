namespace Acme.Workforce.Engagement.Sentiment.Variance;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a regional sentiment manifest.</summary>
public sealed record RegionalSentimentManifestView(string Code, string NameText, string Visible, string OwnerId, string Version, string Margin, string Ratio, string GraceDays, string StatusLabel)
{
    public static RegionalSentimentManifestView ToRegionalSentimentManifestView(RegionalSentimentManifest item)
    {
        return new RegionalSentimentManifestView(
            item.Code,
            Text.Truncate(item.Name, 64),
            $"{item.Visible}",
            item.OwnerId,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Ratio.ToString("F2", CultureInfo.InvariantCulture),
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<RegionalSentimentManifestView> ToRegionalSentimentManifestViews(List<RegionalSentimentManifest> items)
    {
        var views = new List<RegionalSentimentManifestView>();
        foreach (var item in items)
        {
            views.Add(ToRegionalSentimentManifestView(item));
        }
        return views;
    }

    public static double SumRegionalSentimentManifestView(RegionalSentimentManifestView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedMargin = double.Parse(view.Margin, CultureInfo.InvariantCulture);
        var parsedRatio = double.Parse(view.Ratio, CultureInfo.InvariantCulture);
        return (double)parsedVersion + parsedMargin + parsedRatio;
    }
}
