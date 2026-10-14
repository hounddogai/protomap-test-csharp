namespace Acme.Workforce.Engagement.TownHall.Milestone;

using System.Globalization;

/// <summary>How screens and exports show a weighted town hall manifest.</summary>
public sealed record WeightedTownHallManifestView(string Code, string Name, string Version, string DurationMinutes, string Maximum, string CostText, string Summary, string StatusLabel)
{
    public static WeightedTownHallManifestView ToWeightedTownHallManifestView(WeightedTownHallManifest item)
    {
        return new WeightedTownHallManifestView(
            item.Code,
            item.Name,
            item.Version.ToString(CultureInfo.InvariantCulture),
            item.DurationMinutes.ToString(CultureInfo.InvariantCulture),
            item.Maximum.ToString("F2", CultureInfo.InvariantCulture),
            item.Cost.ToString("F2", CultureInfo.InvariantCulture),
            item.Summary,
            $"{item.Status}".ToUpperInvariant());
    }

    public static List<WeightedTownHallManifestView> ToWeightedTownHallManifestViews(List<WeightedTownHallManifest> items)
    {
        var views = new List<WeightedTownHallManifestView>();
        foreach (var item in items)
        {
            views.Add(ToWeightedTownHallManifestView(item));
        }
        return views;
    }

    public static double SumWeightedTownHallManifestView(WeightedTownHallManifestView view)
    {
        var parsedVersion = int.Parse(view.Version, CultureInfo.InvariantCulture);
        var parsedDurationMinutes = int.Parse(view.DurationMinutes, CultureInfo.InvariantCulture);
        var parsedMaximum = double.Parse(view.Maximum, CultureInfo.InvariantCulture);
        return (double)parsedVersion + (double)parsedDurationMinutes + parsedMaximum;
    }
}
