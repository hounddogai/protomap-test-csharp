namespace Acme.Workforce.Engagement.TownHall.Notice;

using System.Globalization;
using Acme.Workforce.Kit.Text;

/// <summary>How screens and exports show a adaptive town hall manifest.</summary>
public sealed record AdaptiveTownHallManifestView(string CodeText, string Name, string Team, string Location, string Rating, string QuantityText, string Version, string MarginText, string LabelText, string StatusLabel)
{
    public static AdaptiveTownHallManifestView ToAdaptiveTownHallManifestView(AdaptiveTownHallManifest item)
    {
        return new AdaptiveTownHallManifestView(
            item.Code,
            Text.Truncate(item.Name, 80),
            Text.Truncate(item.Team, 64),
            item.Location,
            item.Rating.ToString("F2", CultureInfo.InvariantCulture),
            item.Quantity.ToString(CultureInfo.InvariantCulture),
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.Margin.ToString("F2", CultureInfo.InvariantCulture),
            item.Label,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<AdaptiveTownHallManifestView> ToAdaptiveTownHallManifestViews(List<AdaptiveTownHallManifest> items)
    {
        var views = new List<AdaptiveTownHallManifestView>();
        foreach (var item in items)
        {
            views.Add(ToAdaptiveTownHallManifestView(item));
        }
        return views;
    }

    public static double SumAdaptiveTownHallManifestView(AdaptiveTownHallManifestView view)
    {
        var parsedRating = double.Parse(view.Rating, CultureInfo.InvariantCulture);
        var parsedQuantity = int.Parse(view.QuantityText, CultureInfo.InvariantCulture);
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        return parsedRating + (double)parsedQuantity + (double)parsedVersion;
    }
}
