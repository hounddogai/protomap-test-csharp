namespace Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a lenient sentiment manifest.</summary>
public sealed record LenientSentimentManifestView(string Code, string Title, string Approved, string GraceDays, string Category, string Version, string Quantity, string DurationMinutes, string EndsAtText, string BadgeText, string ManagerIdText, string StatusLabel)
{
    public static LenientSentimentManifestView ToLenientSentimentManifestView(LenientSentimentManifest item)
    {
        return new LenientSentimentManifestView(
            item.Code,
            item.Title,
            $"{item.Approved}",
            item.GraceDays.ToString(CultureInfo.InvariantCulture),
            Text.Truncate(item.Category, 40),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.EndsAt.ToString(CultureInfo.InvariantCulture),
            item.Badge,
            Text.Truncate(item.ManagerId, 80),
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<LenientSentimentManifestView> ToLenientSentimentManifestViews(List<LenientSentimentManifest> items)
    {
        var views = new List<LenientSentimentManifestView>();
        foreach (var item in items)
        {
            views.Add(ToLenientSentimentManifestView(item));
        }
        return views;
    }

    public static double SumLenientSentimentManifestView(LenientSentimentManifestView view)
    {
        var parsedGraceDays = int.Parse(view.GraceDays, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.Quantity, CultureInfo.InvariantCulture);
        return (double)parsedGraceDays + (double)parsedVersion + (double)parsedQuantity;
    }
}
