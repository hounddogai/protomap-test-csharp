namespace Acme.Workforce.Engagement.Sentiment.SentimentManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a tiered sentiment manifest.</summary>
public sealed record TieredSentimentManifestView(string Code, string Name, string OwnerId, string TenureMonthsText, string LocaleText, string Rate, string ExpiresAtText, string StatusLabel)
{
    public static TieredSentimentManifestView ToTieredSentimentManifestView(TieredSentimentManifest item)
    {
        return new TieredSentimentManifestView(
            item.Code,
            Text.Truncate(item.Name, 64),
            item.OwnerId,
            item.TenureMonths.ToString(CultureInfo.InvariantCulture),
            item.Locale,
            item.Rate.ToString("F2", CultureInfo.InvariantCulture),
            item.ExpiresAt.ToString(CultureInfo.InvariantCulture),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<TieredSentimentManifestView> ToTieredSentimentManifestViews(List<TieredSentimentManifest> items)
    {
        var views = new List<TieredSentimentManifestView>();
        foreach (var item in items)
        {
            views.Add(ToTieredSentimentManifestView(item));
        }
        return views;
    }

    public static double SumTieredSentimentManifestView(TieredSentimentManifestView view)
    {
        var parsedTenureMonths = int.Parse(view.TenureMonthsText, CultureInfo.InvariantCulture);
        var parsedRate = double.Parse(view.Rate, CultureInfo.InvariantCulture);
        var parsedExpiresAt = int.Parse(view.ExpiresAtText, CultureInfo.InvariantCulture);
        return (double)parsedTenureMonths + parsedRate + (double)parsedExpiresAt;
    }
}
